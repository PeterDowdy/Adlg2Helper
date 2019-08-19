using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Polly;

[assembly:InternalsVisibleTo("Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001001d9881131ed75d2fda704c390b4664f476db93b5e7170fbe6a22441fb1240aa1b3d45acb3f394e13d6dd41cfac6a74ca83a485f14dab4df23eb2fb18d52de9b69291e67e7cf3a70708e049a251b517efdafa14f36edcdd12e9ea6b10f53ed83fea8caafb3f7a258256a26c39a0bca50ffcf9f6d9ffe41598cc6feb51b06b229f")]
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
                return options.AuthorizationMethod == AuthorizationMethod.SharedKey ? Adlg2ClientFactory.BuildPathClient(options.Account, options.Key)
                    : options.AuthorizationMethod == AuthorizationMethod.Oauth ? Adlg2ClientFactory.BuildPathClient(options.Account, options.TenantId, options.ClientId, options.ClientSecret)
                    : options.AuthorizationMethod == AuthorizationMethod.SharedAccessSignature ? Adlg2ClientFactory.BuildPathClientWithSharedAccessSignature(options.Account,options.Sas)
                        : throw new Exception("Authorization method has not been specified. Please use `.AuthorizeWithAccountNameAndKey` or `AuthorizeWithAccountNameAndAzureOauth`.");
            });
            services.AddTransient<Adlg2FilesystemClient>(x =>
            {
                return options.AuthorizationMethod == AuthorizationMethod.SharedKey ? Adlg2ClientFactory.BuildFilesystemClient(options.Account, options.Key) :
                    options.AuthorizationMethod == AuthorizationMethod.Oauth ? Adlg2ClientFactory.BuildFilesystemClient(options.Account, options.TenantId, options.ClientId, options.ClientSecret)
                    : options.AuthorizationMethod == AuthorizationMethod.SharedAccessSignature ? Adlg2ClientFactory.BuildFilesystemClientWithSharedAccessSignature(options.Account, options.Sas)
                        : throw new Exception("Authorization method has not been specified. Please use `.AuthorizeWithAccountNameAndKey` or `AuthorizeWithAccountNameAndAzureOauth`.");
            });
            services.AddHttpClient();
            return services;
        }
    }
    internal enum AuthorizationMethod
    {
        Undefined = -1,
        SharedKey = 0,
        Oauth = 1,
        SharedAccessSignature = 2
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
            Sas = null;
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
            Sas = null;
            AuthorizationMethod = AuthorizationMethod.Oauth;
        }

        public void AuthorizeWithSharedAccessSignature(string account, string sas)
        {
            if (string.IsNullOrEmpty(account)) throw new ArgumentException($"Storage account name may not be null or empty. Storage account name was {(account == null ? "null" : "empty")}.", nameof(account));
            if (string.IsNullOrEmpty(sas)) throw new ArgumentException($"Shared access signature may not be null or empty. Shared key was {(sas == null ? "null" : "empty")}.", nameof(sas));
            if (!sas.StartsWith("&") && !sas.StartsWith("?")) sas = "&" + sas;
            Account = account;
            Sas = sas;
            Key = null;
            ClientId = null;
            ClientSecret = null;
            TenantId = null;
            AuthorizationMethod = AuthorizationMethod.SharedAccessSignature;
        }
        internal AuthorizationMethod AuthorizationMethod = AuthorizationMethod.Undefined;
        internal string Account { get; private set; }
        internal string Key { get; private set; }
        internal string ClientId { get; private set; }
        internal string ClientSecret { get; private set; }
        internal string TenantId { get; private set; }
        internal string Sas { get; private set; }
        #endregion authentication
    }
}