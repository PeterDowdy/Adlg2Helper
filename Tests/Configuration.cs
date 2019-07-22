using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Tests
{
    public static class Configuration
    {
        private static readonly IConfigurationRoot Config;

        static Configuration()
        {
            Config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
        }
        public static string Value(string key)
        {
            return Config[key];
        }
    }
}
