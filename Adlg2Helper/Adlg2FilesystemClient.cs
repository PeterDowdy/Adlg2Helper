using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;

namespace Adlg2Helper
{
    public class Adlg2FilesystemClient
    {
        private readonly string _version = "2018-11-09";
        private readonly string _account;
        private readonly string _key;
        private readonly string _sas;
        private readonly HttpClient _httpClient;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _tenantId;
        private readonly AuthorizationMethod _authorizationMethod;
        private readonly RetryPolicy _retryPolicy = Policy.Handle<AdlOperationTimedOutException>()
            .Or<AuthTokenInvalidException>()
            .Retry(5, (exception, retryCount, context) =>
            {
                if (exception is AuthTokenInvalidException) AzureStorageAuthenticationHelper.ClearToken();
                else Thread.Sleep(TimeSpan.FromSeconds(Math.Pow(2, retryCount)));
            });
        internal Adlg2FilesystemClient(string account, string _, string sas, HttpClient httpClient = null)
        {
            _account = account;
            _sas = sas;
            _httpClient = httpClient ?? Http.Client;
            _authorizationMethod = AuthorizationMethod.SharedAccessSignature;
        }
        internal Adlg2FilesystemClient(string account, string key, HttpClient httpClient = null)
        {
            _account = account;
            _key = key;
            _httpClient = httpClient ?? Http.Client;
            _authorizationMethod = AuthorizationMethod.SharedKey;
        }
        internal Adlg2FilesystemClient(string account, string tenantId, string clientId, string clientSecret, HttpClient httpClient = null)
        {
            _account = account;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _httpClient = httpClient ?? Http.Client;
            _tenantId = tenantId;
            _authorizationMethod = AuthorizationMethod.Oauth;
        }

        private void AssertValidity(string fileName)
        {
            if (fileName == null
            || fileName.Length < 3
                || fileName.Length > 63
            || fileName.Any(f => !"abcdefghijklmnopqrstuvwxyz1234567890-".Contains(f))
            || fileName.StartsWith("-")
            || fileName.EndsWith("-")
            || fileName.Contains("--"))
            throw new ArgumentException("Filesystem name invalid; The value must start and end with a letter or number and must contain only letters, numbers, and the dash (-) character. Consecutive dashes are not permitted. All letters must be lowercase. The value must have between 3 and 63 characters..");
        }
        public bool Create(string filesystem)
        {
            AssertValidity(filesystem);
            var parameters = new List<string>
            {
                $"resource=filesystem".ToLowerInvariant()
            };
            return _retryPolicy.Execute(() =>
            {
                using (var request = new HttpRequestMessage(HttpMethod.Put,
                    $"https://{_account}.dfs.core.windows.net/{filesystem}?{string.Join("&", parameters)}"))
                {
                    DateTime now = DateTime.UtcNow;
                    request.Headers.Add("x-ms-date", now.ToString("R", CultureInfo.InvariantCulture));
                    request.Headers.Add("x-ms-version", _version);
                    request.Headers.Authorization = _authorizationMethod == AuthorizationMethod.SharedKey ? AzureStorageAuthenticationHelper.BuildSignedAuthorizationHeader(_account, _key, now, request)
                        : _authorizationMethod == AuthorizationMethod.Oauth ? AzureStorageAuthenticationHelper.BuildBearerTokenHeader(_httpClient, _tenantId, _clientId, _clientSecret)
                        : null;
                    if (_authorizationMethod == AuthorizationMethod.SharedAccessSignature) request.RequestUri = new Uri(request.RequestUri + _sas.Replace('?','&'));
                    using (HttpResponseMessage response =
                        _httpClient.SendAsync(request).GetAwaiter().GetResult())
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            if (response.StatusCode == HttpStatusCode.Conflict) return false;
                            if (response.StatusCode == HttpStatusCode.Unauthorized &&
                                _authorizationMethod == AuthorizationMethod.Oauth)
                                throw new AuthTokenInvalidException();
                            throw new Exception(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                        }

                        return response.StatusCode == HttpStatusCode.Created;
                    }
                }
            });
        }
        public bool Delete(string filesystem)
        {
            AssertValidity(filesystem);
            var parameters = new List<string>
            {
                $"resource=filesystem".ToLowerInvariant()
            };
            return _retryPolicy.Execute(() =>
            {
                using (var request = new HttpRequestMessage(HttpMethod.Delete,
                    $"https://{_account}.dfs.core.windows.net/{filesystem}?{string.Join("&", parameters)}"))
                {
                    DateTime now = DateTime.UtcNow;
                    request.Headers.Add("x-ms-date", now.ToString("R", CultureInfo.InvariantCulture));
                    request.Headers.Add("x-ms-version", _version);
                    request.Headers.Authorization = _authorizationMethod == AuthorizationMethod.SharedKey ? AzureStorageAuthenticationHelper.BuildSignedAuthorizationHeader(_account, _key, now, request)
                        : _authorizationMethod == AuthorizationMethod.Oauth ? AzureStorageAuthenticationHelper.BuildBearerTokenHeader(_httpClient, _tenantId, _clientId, _clientSecret)
                        : null;
                    if (_authorizationMethod == AuthorizationMethod.SharedAccessSignature) request.RequestUri = new Uri(request.RequestUri + _sas.Replace('?','&'));
                    using (HttpResponseMessage response =
                        _httpClient.SendAsync(request).GetAwaiter().GetResult())
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            if (response.StatusCode == HttpStatusCode.Conflict) return false;
                            if (response.StatusCode == HttpStatusCode.NotFound) return false;
                            if (response.StatusCode == HttpStatusCode.Unauthorized &&
                                _authorizationMethod == AuthorizationMethod.Oauth)
                                throw new AuthTokenInvalidException();
                            throw new Exception(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                        }

                        return response.StatusCode == HttpStatusCode.Accepted;
                    }
                }
            });
        }
        public IEnumerable<AdlFilesystem> List(bool recursive = false, string prefix = null, string continuation = null, int maxResults = 5000, int? timeout = null)
        {
            var parameters = new List<string>
            {
                $"recursive={recursive}".ToLowerInvariant(),
                $"resource=account",
                $"maxResults={maxResults}".ToLowerInvariant()
            };
            if (!string.IsNullOrEmpty(prefix)) parameters.Add($"prefix={HttpUtility.UrlEncode(prefix)}");
            if (!string.IsNullOrEmpty(continuation)) parameters.Add($"continuation={HttpUtility.UrlEncode(continuation)}");
            if (timeout.HasValue) parameters.Add($"timeout={timeout}");
            return _retryPolicy.Execute(() => {
                using (var request = new HttpRequestMessage(HttpMethod.Get, $"https://{_account}.dfs.core.windows.net/?{string.Join("&", parameters)}"))
                {
                    DateTime now = DateTime.UtcNow;
                    request.Headers.Add("x-ms-date", now.ToString("R", CultureInfo.InvariantCulture));
                    request.Headers.Add("x-ms-version", _version);
                    request.Headers.Authorization = _authorizationMethod == AuthorizationMethod.SharedKey ? AzureStorageAuthenticationHelper.BuildSignedAuthorizationHeader(_account, _key, now, request)
                        : _authorizationMethod == AuthorizationMethod.Oauth ? AzureStorageAuthenticationHelper.BuildBearerTokenHeader(_httpClient, _tenantId, _clientId, _clientSecret)
                        : null;
                    if (_authorizationMethod == AuthorizationMethod.SharedAccessSignature) request.RequestUri = new Uri(request.RequestUri + _sas.Replace('?','&'));
                    using (var response = _httpClient.SendAsync(request).GetAwaiter().GetResult())
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            if (response.StatusCode == HttpStatusCode.NotFound) return Enumerable.Empty<AdlFilesystem>();
                            if (response.StatusCode == HttpStatusCode.InternalServerError && response.Content
                                    .ReadAsStringAsync().GetAwaiter().GetResult()
                                    .Contains("Operation could not be completed within the specified time."))
                                throw new AdlOperationTimedOutException();
                            if (response.StatusCode == HttpStatusCode.Unauthorized && _authorizationMethod == AuthorizationMethod.Oauth) throw new AuthTokenInvalidException();
                            throw new AdlUnexpectedException(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                        }
                        var result = JsonConvert.DeserializeObject<AdlFilesystemList>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                        var continuationHeader = response.Headers.Any(h => h.Key == "x-ms-continuation") ? response.Headers.Single(h => h.Key == "x-ms-continuation").Value.Single() : null;
                        if (!string.IsNullOrEmpty(continuationHeader))
                        {
                            continuation = continuationHeader;
                            var continuedPaths = List(recursive, prefix, continuation, maxResults);
                            return result.Filesystems.Concat(continuedPaths);
                        }

                        return result.Filesystems;
                    }
                }
            });
        }
        public AdlFilesystemProperties GetProperties(string filesystem)
        {
            AssertValidity(filesystem);
            var parameters = new List<string>
            {
                $"resource=filesystem"
            };
            return _retryPolicy.Execute(() =>
            {
                using (var request = new HttpRequestMessage(HttpMethod.Head,
                    $"https://{_account}.dfs.core.windows.net/{filesystem}?{string.Join("&", parameters)}"))
                {
                    DateTime now = DateTime.UtcNow;
                    request.Headers.Add("x-ms-date", now.ToString("R", CultureInfo.InvariantCulture));
                    request.Headers.Add("x-ms-version", _version);
                    request.Headers.Authorization = _authorizationMethod == AuthorizationMethod.SharedKey ? AzureStorageAuthenticationHelper.BuildSignedAuthorizationHeader(_account, _key, now, request)
                        : _authorizationMethod == AuthorizationMethod.Oauth ? AzureStorageAuthenticationHelper.BuildBearerTokenHeader(_httpClient, _tenantId, _clientId, _clientSecret)
                        : null;
                    if (_authorizationMethod == AuthorizationMethod.SharedAccessSignature) request.RequestUri = new Uri(request.RequestUri + _sas.Replace('?','&'));
                    using (var response = _httpClient.SendAsync(request).GetAwaiter().GetResult())
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            if (response.StatusCode == HttpStatusCode.InternalServerError && response.Content
                                    .ReadAsStringAsync().GetAwaiter().GetResult()
                                    .Contains("Operation could not be completed within the specified time."))
                                throw new AdlOperationTimedOutException();
                            if (response.StatusCode == HttpStatusCode.NotFound) return null;
                            throw new Exception(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                        }

                        return new AdlFilesystemProperties
                        {
                            NamespaceEnabled = response.Headers.SingleOrDefault(h => h.Key == "x-ms-namespace-enabled")
                                .Value?.FirstOrDefault(),
                            Properties = response.Headers.SingleOrDefault(h => h.Key == "x-ms-properties").Value
                                ?.FirstOrDefault()
                        };
                    }
                }
            });
        }
        public bool SetProperties(string filesystem, Dictionary<string,string> properties = null)
        {
            AssertValidity(filesystem);
            var parameters = new List<string>
            {
                $"resource=filesystem"
            };
            return _retryPolicy.Execute(() =>
            {
                using (var request = new HttpRequestMessage(HttpMethod.Patch,
                    $"https://{_account}.dfs.core.windows.net/{filesystem}?{string.Join("&", parameters)}"))
                {
                    DateTime now = DateTime.UtcNow;
                    request.Headers.Add("x-ms-date", now.ToString("R", CultureInfo.InvariantCulture));
                    request.Headers.Add("x-ms-version", _version);
                    if (properties?.Any() ?? false)
                        request.Headers.Add("x-ms-properties",
                            string.Join(",",
                                properties.Select(kvp =>
                                    $"{kvp.Key}={Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(kvp.Value))}")));
                    request.Headers.Authorization = _authorizationMethod == AuthorizationMethod.SharedKey ? AzureStorageAuthenticationHelper.BuildSignedAuthorizationHeader(_account, _key, now, request)
                        : _authorizationMethod == AuthorizationMethod.Oauth ? AzureStorageAuthenticationHelper.BuildBearerTokenHeader(_httpClient, _tenantId, _clientId, _clientSecret)
                        : null;
                    if (_authorizationMethod == AuthorizationMethod.SharedAccessSignature) request.RequestUri = new Uri(request.RequestUri + _sas.Replace('?','&'));
                    using (var response = _httpClient.SendAsync(request).GetAwaiter().GetResult())
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            if (response.StatusCode == HttpStatusCode.InternalServerError && response.Content
                                    .ReadAsStringAsync().GetAwaiter().GetResult()
                                    .Contains("Operation could not be completed within the specified time."))
                                throw new AdlOperationTimedOutException();
                            return false;
                        }

                        return true;
                    }
                }
            });
        }
    }

    public class AdlFilesystemProperties
    {
        public string NamespaceEnabled { get; set; }
        public string Properties { get; set; }
    }
}