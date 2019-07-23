using System;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace Adlg2Helper
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAzureDataLakeGen2Client(this IServiceCollection services, string account, string key)
        {
            services.AddTransient<Adlg2PathClient>(x =>
            {
                var options = new AzureDataLakeGen2Options();
                options.AuthorizeWithAccountNameAndKey(account,key);
                return Adlg2ClientFactory.BuildPathClient(options.Account, options.Key);
            });
            services.AddTransient<Adlg2FilesystemClient>(x =>
            {
                var options = new AzureDataLakeGen2Options();
                options.AuthorizeWithAccountNameAndKey(account, key);
                return Adlg2ClientFactory.BuildFilesystemClient(options.Account, options.Key);
            });
            return services;
        }
        public static IServiceCollection AddAzureDataLakeGen2Client(this IServiceCollection services, Action<AzureDataLakeGen2Options> optionsBuilder)
        {
            var options = new AzureDataLakeGen2Options();
            optionsBuilder(options);
            return services.AddAzureDataLakeGen2Client(options.Account, options.Key);
        }
    }

    public class AzureDataLakeGen2Options
    {
        public void AuthorizeWithAccountNameAndKey(string account, string key)
        {
            if (string.IsNullOrEmpty(account)) throw new ArgumentException($"Storage account name may not be null or empty. Storage account name was {(account == null ? "null" : "empty")}.", nameof(account));
            if (string.IsNullOrEmpty(key)) throw new ArgumentException($"Shared key may not be null or empty. Shared key was {(key == null ? "null" : "empty")}.", nameof(key));
            Account = account;
            Key = key;
        }
        internal string Account { get; private set; }
        internal string Key { get; private set; }
    }
}