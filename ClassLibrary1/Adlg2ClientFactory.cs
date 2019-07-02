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
    }
}
