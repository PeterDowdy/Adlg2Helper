using Microsoft.Extensions.Configuration;

namespace Tests
{
    public static class Configuration
    {
        private static readonly IConfigurationRoot Config;

        static Configuration()
        {
            Config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }
        public static string Value(string key)
        {
            return System.Environment.GetEnvironmentVariable(key) ?? Config[key];
        }
    }
}
