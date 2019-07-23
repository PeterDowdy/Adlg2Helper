﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Polly;

namespace Adlg2Helper
{
    public class AdlPathProperties
    {
        public string ResourceType { get; set; }
        public string Properties { get; set; }
        public string Owner { get; set; }
        public string Group { get; set; }
        public string Permissions { get; set; }
        public string Acl { get; set; }
        public string LeaseDuration { get; set; }
        public string LeaseState { get; set; }
        public string LeaseStatus { get; set; }
    }
    public class AdlFilesystem
    {
        public string ETag { get; set; }
        public string LastModified { get; set; }
        public string Name { get; set; }
    }
    public class AdlFilesystemList
    {
        public IEnumerable<AdlFilesystem> Filesystems { get; set; }
    }
    public class AdlPath
    {
        public int ContentLength { get; set; }
        public string ETag { get; set; }
        public string Group { get; set; }
        public bool IsDirectory { get; set; }
        public string LastModified { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }
        public string Permissions { get; set; }
    }
    public class AdlPathList
    {
        public IEnumerable<AdlPath> Paths { get; set; }
    }
    public class Adlg2PathClient
    {
        private readonly string _version = "2018-11-09";
        private readonly string _account;
        private readonly string _key;

        internal Adlg2PathClient(string account, string key)
        {
            _account = account;
            _key = key;
        }

        public bool Create(string filesystem, string path, string resource, bool overWrite)
        {
            var parameters = new List<string>
            {
                $"resource={resource}".ToLowerInvariant()
            };
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put,
                $"https://{_account}.dfs.core.windows.net/{filesystem}/{path}?{string.Join("&", parameters)}"))
            {
                DateTime now = DateTime.UtcNow;
                httpRequestMessage.Headers.Add("x-ms-date", now.ToString("R", CultureInfo.InvariantCulture));
                httpRequestMessage.Headers.Add("x-ms-version", _version);
                if (!overWrite) httpRequestMessage.Headers.Add("If-None-Match", "*");
                httpRequestMessage.Headers.Authorization = AzureStorageAuthenticationHelper.BuildSignedAuthorizationHeader(_account, _key, now, httpRequestMessage, ifNoneMatch: !overWrite ? "*" : null);
                using (HttpResponseMessage httpResponseMessage = Http.Client.SendAsync(httpRequestMessage).GetAwaiter().GetResult())
                {
                    if (!httpResponseMessage.IsSuccessStatusCode)
                    {
                        if (httpResponseMessage.StatusCode == HttpStatusCode.Conflict) return false;
                        throw new Exception(httpResponseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                    }
                    return httpResponseMessage.StatusCode == HttpStatusCode.Created;
                }
            }
        }
        public bool Delete(string filesystem, string path, bool recursive, string continuation = null)
        {
            var parameters = new List<string>
            {
                $"recursive={recursive}".ToLowerInvariant()
            };
            if (!string.IsNullOrEmpty(continuation)) parameters.Add($"continuation={HttpUtility.UrlEncode(continuation)}");
            using (var request = new HttpRequestMessage(HttpMethod.Delete, $"https://{_account}.dfs.core.windows.net/{filesystem}/{path}?{string.Join("&", parameters)}"))
            {
                DateTime now = DateTime.UtcNow;
                request.Headers.Add("x-ms-date", now.ToString("R", CultureInfo.InvariantCulture));
                request.Headers.Add("x-ms-version", _version);
                request.Headers.Authorization = AzureStorageAuthenticationHelper.BuildSignedAuthorizationHeader(_account, _key, now, request);
                using (var response = Http.Client.SendAsync(request).GetAwaiter().GetResult())
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == HttpStatusCode.NotFound) return false;
                        if (response.StatusCode == HttpStatusCode.Conflict) return false;
                        throw new Exception(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                    }
                    var continuationHeader = response.Headers.Any(h => h.Key == "x-ms-continuation") ? response.Headers.Single(h => h.Key == "x-ms-continuation").Value.Single() : null;
                    if (!string.IsNullOrEmpty(continuationHeader))
                    {
                        return Delete(filesystem, path, recursive, continuationHeader);
                    }

                    return true;
                }
            }
        }
        public object GetProperties()
        {
            throw new NotImplementedException();
        }
        public bool Lease(string filesystem, string path, string action, out string returnedLeaseId, string proposedLeaseId = null, string leaseId = null, int? leaseDuration = null, int? leaseBreakPeriod = null)
        {
            if (leaseDuration != null && ((leaseDuration < 15 && leaseDuration != -1) || leaseDuration > 60)) throw new ArgumentException($"Lease duration is invalid. Valid lease durations are -1 and 15-60. Provided {leaseDuration}.",nameof(leaseDuration));
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, $"https://{_account}.dfs.core.windows.net/{filesystem}/{path}"))
            {
                DateTime now = DateTime.UtcNow;
                httpRequestMessage.Headers.Add("x-ms-lease-action",action);
                if (leaseDuration != null) httpRequestMessage.Headers.Add("x-ms-lease-duration",leaseDuration.ToString());
                if (leaseBreakPeriod != null) httpRequestMessage.Headers.Add("x-ms-lease-break-period", leaseBreakPeriod.ToString());
                if (!string.IsNullOrEmpty(proposedLeaseId)) httpRequestMessage.Headers.Add("x-ms-proposed-lease-id", proposedLeaseId);
                if (!string.IsNullOrEmpty(leaseId)) httpRequestMessage.Headers.Add("x-ms-lease-id", leaseId);
                httpRequestMessage.Headers.Add("x-ms-date", now.ToString("R", CultureInfo.InvariantCulture));
                httpRequestMessage.Headers.Add("x-ms-version", _version);
                httpRequestMessage.Headers.Authorization = AzureStorageAuthenticationHelper.BuildSignedAuthorizationHeader(
                    _account,
                    _key,
                    now,
                    httpRequestMessage
                );
                using (var response = Http.Client.SendAsync(httpRequestMessage).GetAwaiter().GetResult())
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            returnedLeaseId = null;
                            return false;
                        }
                        if (response.StatusCode == HttpStatusCode.Conflict)
                        {
                            returnedLeaseId = null;
                            return false;
                        }
                        throw new Exception(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                    }

                    returnedLeaseId = response.Headers.Any(h => h.Key == "x-ms-lease-id")
                        ? response.Headers.Single(h => h.Key == "x-ms-lease-id").Value.Single()
                        : null;
                    return true;
                }
            }
        }
        public IEnumerable<AdlPath> List(string filesystem, bool recursive = false, string directory = null, string continuation = null, int maxResults = 5000, int? timeout = null)
        {
            var retryPolicy = Policy.Handle<AdlOperationTimedOutException>().WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
            var parameters = new List<string>
            {
                $"recursive={recursive}".ToLowerInvariant(),
                $"resource=filesystem",
                $"maxResults={maxResults}".ToLowerInvariant()
            };
            if (!string.IsNullOrEmpty(directory)) parameters.Add($"directory={HttpUtility.UrlEncode(directory)}");
            if (!string.IsNullOrEmpty(continuation)) parameters.Add($"continuation={HttpUtility.UrlEncode(continuation)}");
            if (timeout.HasValue) parameters.Add($"timeout={timeout}");
            return retryPolicy.Execute(() => {
            using (var request = new HttpRequestMessage(HttpMethod.Get, $"https://{_account}.dfs.core.windows.net/{filesystem}?{string.Join("&",parameters)}"))
            {
                DateTime now = DateTime.UtcNow;
                request.Headers.Add("x-ms-date", now.ToString("R", CultureInfo.InvariantCulture));
                request.Headers.Add("x-ms-version", _version);
                request.Headers.Authorization = AzureStorageAuthenticationHelper.BuildSignedAuthorizationHeader(_account, _key, now, request);
                using (var response = Http.Client.SendAsync(request).GetAwaiter().GetResult())
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == HttpStatusCode.NotFound) return Enumerable.Empty<AdlPath>();
                        if (response.StatusCode == HttpStatusCode.InternalServerError && response.Content
                                .ReadAsStringAsync().GetAwaiter().GetResult()
                                .Contains("Operation could not be completed within the specified time."))
                            throw new AdlOperationTimedOutException();
                        throw new AdlUnexpectedException(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                    }
                    var result = JsonConvert.DeserializeObject<AdlPathList>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                    var continuationHeader = response.Headers.Any(h => h.Key == "x-ms-continuation") ? response.Headers.Single(h => h.Key == "x-ms-continuation").Value.Single() : null;
                    if (!string.IsNullOrEmpty(continuationHeader))
                    {
                        continuation = continuationHeader;
                        var continuedPaths = List(filesystem, recursive, directory, continuation, maxResults);
                        return result.Paths.Concat(continuedPaths);
                    }

                    return result.Paths;
                }
            }
            });
        }
        public Stream ReadStream(string filesystem, string path, long rangeStart, long rangeStop, int? timeout = null)
        {
            var retryPolicy = Policy.Handle<AdlOperationTimedOutException>().WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
            return retryPolicy.Execute(() =>
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get,
                    $"https://{_account}.dfs.core.windows.net/{filesystem}/{path}"))
                {
                    DateTime now = DateTime.UtcNow;
                    request.Headers.Add("x-ms-date", now.ToString("R", CultureInfo.InvariantCulture));
                    request.Headers.Add("x-ms-version", _version);
                    request.Headers.Add("Range", $"bytes={rangeStart}-{rangeStop}");
                    request.Headers.Authorization =
                        AzureStorageAuthenticationHelper.BuildSignedAuthorizationHeader(_account, _key, now, request,
                            range: $"bytes={rangeStart}-{rangeStop}");
                    using (var response = Http.Client.SendAsync(request).GetAwaiter().GetResult())
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            if (response.StatusCode == HttpStatusCode.InternalServerError && response.Content
                                    .ReadAsStringAsync().GetAwaiter().GetResult()
                                    .Contains("Operation could not be completed within the specified time."))
                                throw new AdlOperationTimedOutException();
                            throw new Exception(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                        }
                        var ms = new MemoryStream();
                        response.Content.ReadAsStreamAsync().GetAwaiter().GetResult().CopyTo(ms);
                        return ms;
                    }
                }
            });
        }
        public Byte[] ReadBytes(string filesystem, string path, long rangeStart, long rangeStop, int? timeout = null)
        {
            var retryPolicy = Policy.Handle<AdlOperationTimedOutException>().WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
            return retryPolicy.Execute(() =>
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get,
                    $"https://{_account}.dfs.core.windows.net/{filesystem}/{path}"))
                {
                    DateTime now = DateTime.UtcNow;
                    request.Headers.Add("x-ms-date", now.ToString("R", CultureInfo.InvariantCulture));
                    request.Headers.Add("x-ms-version", _version);
                    request.Headers.Add("Range", $"bytes={rangeStart}-{rangeStop}");
                    request.Headers.Authorization =
                        AzureStorageAuthenticationHelper.BuildSignedAuthorizationHeader(_account, _key, now, request,
                            range: $"bytes={rangeStart}-{rangeStop}");
                    using (var response = Http.Client.SendAsync(request).GetAwaiter().GetResult())
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            if (response.StatusCode == HttpStatusCode.InternalServerError && response.Content
                                    .ReadAsStringAsync().GetAwaiter().GetResult()
                                    .Contains("Operation could not be completed within the specified time."))
                                throw new AdlOperationTimedOutException();
                            throw new Exception(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                        }
                        return response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                    }
                }
            });
        }
        public bool Update(string filesystem, string path, string action, byte[] content = null, long? position = 0, bool? close = null)
        {
            if (action.Equals("flush", StringComparison.InvariantCultureIgnoreCase) && !position.HasValue) throw new ArgumentException("Action `flush` must be performed with a position parameter.");
            var retryPolicy = Policy.Handle<AdlOperationTimedOutException>().WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
            var parameters = new List<string>
            {
                $"action={action}".ToLowerInvariant()
            };
            if (position.HasValue) parameters.Add($"position={position}");
            if (close.HasValue) parameters.Add($"close={close}".ToLowerInvariant());
            return retryPolicy.Execute(() =>
            {
                using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Patch,
                    $"https://{_account}.dfs.core.windows.net/{filesystem}/{path}?{string.Join("&", parameters)}"))
                {
                    if (content != null) httpRequestMessage.Content = new ByteArrayContent(content);
                    DateTime now = DateTime.UtcNow;
                    httpRequestMessage.Headers.Add("x-ms-date", now.ToString("R", CultureInfo.InvariantCulture));
                    httpRequestMessage.Headers.Add("x-ms-version", _version);
                    httpRequestMessage.Headers.Authorization =
                        AzureStorageAuthenticationHelper.BuildSignedAuthorizationHeader(
                            _account,
                            _key,
                            now,
                            httpRequestMessage,
                            contentLength: content?.LongLength
                        );
                    using (HttpResponseMessage response =
                        Http.Client.SendAsync(httpRequestMessage).GetAwaiter().GetResult())
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            if (response.StatusCode == HttpStatusCode.InternalServerError && response.Content
                                    .ReadAsStringAsync().GetAwaiter().GetResult()
                                    .Contains("Operation could not be completed within the specified time."))
                                throw new AdlOperationTimedOutException();
                            throw new Exception(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                        }

                        return true;
                    }
                }
            });
        }

        public AdlPathProperties GetProperties(string filesystem, string path, string action = null, string upn = null)
        {
            var parameters = new List<string>();
            if (!string.IsNullOrEmpty(action)) parameters.Add($"action={action}");
            if (!string.IsNullOrEmpty(upn)) parameters.Add($"upn={upn}".ToLowerInvariant());

                using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Head,
                    $"https://{_account}.dfs.core.windows.net/{filesystem}/{path}?{string.Join("&", parameters)}"))
                {
                    DateTime now = DateTime.UtcNow;
                    httpRequestMessage.Headers.Add("x-ms-date", now.ToString("R", CultureInfo.InvariantCulture));
                    httpRequestMessage.Headers.Add("x-ms-version", _version);
                    httpRequestMessage.Headers.Authorization =
                        AzureStorageAuthenticationHelper.BuildSignedAuthorizationHeader(
                            _account,
                            _key,
                            now,
                            httpRequestMessage
                        );
                    using (HttpResponseMessage response =
                        Http.Client.SendAsync(httpRequestMessage).GetAwaiter().GetResult())
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            if (response.StatusCode == HttpStatusCode.InternalServerError && response.Content
                                    .ReadAsStringAsync().GetAwaiter().GetResult()
                                    .Contains("Operation could not be completed within the specified time."))
                                throw new AdlOperationTimedOutException();
                            throw new Exception(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                        }

                        return new AdlPathProperties
                        {
                            Acl = response.Headers.SingleOrDefault(h => h.Key == "x-ms-acl").Value?.FirstOrDefault(),
                            Group = response.Headers.SingleOrDefault(h => h.Key == "x-ms-group").Value?.FirstOrDefault(),
                            LeaseDuration = response.Headers.SingleOrDefault(h => h.Key == "x-ms-lease-duration").Value?.FirstOrDefault(),
                            LeaseState = response.Headers.SingleOrDefault(h => h.Key == "x-ms-lease-state").Value?.FirstOrDefault(),
                            LeaseStatus = response.Headers.SingleOrDefault(h => h.Key == "x-ms-lease-status").Value?.FirstOrDefault(),
                            Owner = response.Headers.SingleOrDefault(h => h.Key == "x-ms-owner").Value?.FirstOrDefault(),
                            Permissions = response.Headers.SingleOrDefault(h => h.Key == "x-ms-permissions").Value?.FirstOrDefault(),
                            Properties = response.Headers.SingleOrDefault(h => h.Key == "x-ms-properties").Value?.FirstOrDefault(),
                            ResourceType = response.Headers.SingleOrDefault(h => h.Key == "x-ms-resource-type").Value?.FirstOrDefault(),
                        };
                    }
                }
        }
    }

    public class AdlOperationTimedOutException : Exception
    {
        public AdlOperationTimedOutException() : base() { }
        public AdlOperationTimedOutException(string message) : base(message) { }
        public AdlOperationTimedOutException(string message, Exception innerException) : base(message,innerException) { }
    }

    public class AdlUnexpectedException : Exception
    {
        public AdlUnexpectedException() : base() { }
        public AdlUnexpectedException(string message) : base(message) { }
        public AdlUnexpectedException(string message, Exception innerException) : base(message, innerException) { }
    }
}