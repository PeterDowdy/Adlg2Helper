using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Polly;

namespace Adlg2Helper
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAzureDataLakeGen2Client(this IServiceCollection services, string account, string key)
        {
            return AddAzureDataLakeGen2Client(services, options => { options.AuthorizeWithAccountNameAndKey(account, key); });
        }
        public static IServiceCollection AddAzureDataLakeGen2Client(this IServiceCollection services, string account, string tenantId, string clientId, string clientSecret)
        {
            return AddAzureDataLakeGen2Client(services, options => { options.AuthorizeWithAccountNameAndAzureOauth(account, tenantId, clientId, clientSecret); });
        }
        public static IServiceCollection AddAzureDataLakeGen2Client(this IServiceCollection services, Action<AzureDataLakeGen2Options> optionsBuilder)
        {
            var options = new AzureDataLakeGen2Options();
            optionsBuilder(options);
            services.AddTransient<Adlg2PathClient>(x =>
            {
                return options.AuthorizationMethod == AuthorizationMethod.SharedKey
                    ? Adlg2ClientFactory.BuildPathClient(options.Account, options.Key) :
                    options.AuthorizationMethod == AuthorizationMethod.Oauth
                        ? Adlg2ClientFactory.BuildPathClient(options.Account, options.TenantId, options.ClientId, options.ClientSecret)
                        : throw new Exception("Authorization method has not been specified. Please use `.AuthorizeWithAccountNameAndKey` or `AuthorizeWithAccountNameAndAzureOauth`.");
            });
            services.AddTransient<Adlg2FilesystemClient>(x =>
            {
                return options.AuthorizationMethod == AuthorizationMethod.SharedKey
                    ? Adlg2ClientFactory.BuildFilesystemClient(options.Account, options.Key) :
                    options.AuthorizationMethod == AuthorizationMethod.Oauth
                        ? Adlg2ClientFactory.BuildFilesystemClient(options.Account, options.TenantId, options.ClientId, options.ClientSecret)
                        : throw new Exception("Authorization method has not been specified. Please use `.AuthorizeWithAccountNameAndKey` or `AuthorizeWithAccountNameAndAzureOauth`."); ;
            });
            services.AddHttpClient();
            return services;
        }
    }
    internal enum AuthorizationMethod
    {
        Undefined = -1,
        SharedKey = 0,
        Oauth = 1
    }
    public class AzureDataLakeGen2Options
    {
        #region authentication
        public void AuthorizeWithAccountNameAndKey(string account, string key)
        {
            if (string.IsNullOrEmpty(account)) throw new ArgumentException($"Storage account name may not be null or empty. Storage account name was {(account == null ? "null" : "empty")}.", nameof(account));
            if (string.IsNullOrEmpty(key)) throw new ArgumentException($"Shared key may not be null or empty. Shared key was {(key == null ? "null" : "empty")}.", nameof(key));
            Account = account;
            Key = key;
            ClientId = null;
            ClientSecret = null;
            TenantId = null;
            AuthorizationMethod = AuthorizationMethod.SharedKey;
        }

        public void AuthorizeWithAccountNameAndAzureOauth(string account, string tenantId, string clientId, string clientSecret)
        {
            if (string.IsNullOrEmpty(account)) throw new ArgumentException($"Storage account name may not be null or empty. Storage account name was {(account == null ? "null" : "empty")}.", nameof(account));
            if (string.IsNullOrEmpty(tenantId)) throw new ArgumentException($"Tenant id may not be null or empty. Shared key was {(tenantId == null ? "null" : "empty")}.", nameof(tenantId));
            if (string.IsNullOrEmpty(clientId)) throw new ArgumentException($"Client id may not be null or empty. Storage account name was {(clientId == null ? "null" : "empty")}.", nameof(clientId));
            if (string.IsNullOrEmpty(clientSecret)) throw new ArgumentException($"Client secret may not be null or empty. Shared key was {(clientSecret == null ? "null" : "empty")}.", nameof(clientSecret));
            Account = account;
            ClientId = clientId;
            ClientSecret = clientSecret;
            TenantId = tenantId;
            Key = null;
            AuthorizationMethod = AuthorizationMethod.Oauth;
        }
        internal AuthorizationMethod AuthorizationMethod = AuthorizationMethod.Undefined;
        internal string Account { get; private set; }
        internal string Key { get; private set; }
        internal string ClientId { get; private set; }
        internal string ClientSecret { get; private set; }
        internal string TenantId { get; private set; }
        #endregion authentication
    }
}