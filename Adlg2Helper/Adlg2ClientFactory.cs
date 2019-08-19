using System;

namespace Adlg2Helper
{
    public static class Adlg2ClientFactory
    {
        public static Adlg2PathClient BuildPathClient(string account, string key)
        {
            if (string.IsNullOrEmpty(account)) throw new ArgumentException($"Storage account name may not be null or empty. Storage account name was {(account == null ? "null" : "empty")}.", nameof(account));
            if (string.IsNullOrEmpty(key)) throw new ArgumentException($"Shared key may not be null or empty. Shared key was {(key == null ? "null" : "empty")}.", nameof(key));
            return new Adlg2PathClient(account, key);
        }
        internal static Adlg2PathClient BuildPathClientWithSharedAccessSignature(string account, string sas)
        {
            if (string.IsNullOrEmpty(account)) throw new ArgumentException($"Storage account name may not be null or empty. Storage account name was {(account == null ? "null" : "empty")}.", nameof(account));
            if (string.IsNullOrEmpty(sas)) throw new ArgumentException($"Shared access signature may not be null or empty. Shared key was {(sas == null ? "null" : "empty")}.", nameof(sas));
            return new Adlg2PathClient(account, null, sas);
        }
        public static Adlg2PathClient BuildPathClient(string account, string tenantId, string clientId, string clientSecret)
        {
            if (string.IsNullOrEmpty(account)) throw new ArgumentException($"Storage account name may not be null or empty. Storage account name was {(account == null ? "null" : "empty")}.", nameof(account));
            if (string.IsNullOrEmpty(tenantId)) throw new ArgumentException($"Tenant id may not be null or empty. Shared key was {(tenantId == null ? "null" : "empty")}.", nameof(tenantId));
            if (string.IsNullOrEmpty(clientId)) throw new ArgumentException($"Client id may not be null or empty. Shared key was {(clientId == null ? "null" : "empty")}.", nameof(clientId));
            if (string.IsNullOrEmpty(clientSecret)) throw new ArgumentException($"Client secret may not be null or empty. Shared key was {(clientSecret == null ? "null" : "empty")}.", nameof(clientSecret));
            return new Adlg2PathClient(account, tenantId, clientId, clientSecret);
        }
        public static Adlg2FilesystemClient BuildFilesystemClient(string account, string key)
        {
            if (string.IsNullOrEmpty(account)) throw new ArgumentException($"Storage account name may not be null or empty. Storage account name was {(account == null ? "null" : "empty")}.", nameof(account));
            if (string.IsNullOrEmpty(key)) throw new ArgumentException($"Shared key may not be null or empty. Shared key was {(key == null ? "null" : "empty")}.", nameof(key));
            return new Adlg2FilesystemClient(account, key);
        }
        internal static Adlg2FilesystemClient BuildFilesystemClientWithSharedAccessSignature(string account, string sas)
        {
            if (string.IsNullOrEmpty(account)) throw new ArgumentException($"Storage account name may not be null or empty. Storage account name was {(account == null ? "null" : "empty")}.", nameof(account));
            if (string.IsNullOrEmpty(sas)) throw new ArgumentException($"Shared access signature may not be null or empty. Shared key was {(sas == null ? "null" : "empty")}.", nameof(sas));
            return new Adlg2FilesystemClient(account, null, sas);
        }
        public static Adlg2FilesystemClient BuildFilesystemClient(string account, string tenantId, string clientId, string clientSecret)
        {
            if (string.IsNullOrEmpty(account)) throw new ArgumentException($"Storage account name may not be null or empty. Storage account name was {(account == null ? "null" : "empty")}.", nameof(account));
            if (string.IsNullOrEmpty(tenantId)) throw new ArgumentException($"Tenant id may not be null or empty. Shared key was {(tenantId == null ? "null" : "empty")}.", nameof(tenantId));
            if (string.IsNullOrEmpty(clientId)) throw new ArgumentException($"Client id may not be null or empty. Shared key was {(clientId == null ? "null" : "empty")}.", nameof(clientId));
            if (string.IsNullOrEmpty(clientSecret)) throw new ArgumentException($"Client secret may not be null or empty. Shared key was {(clientSecret == null ? "null" : "empty")}.", nameof(clientSecret));
            return new Adlg2FilesystemClient(account, tenantId, clientId, clientSecret);
        }
    }
}
