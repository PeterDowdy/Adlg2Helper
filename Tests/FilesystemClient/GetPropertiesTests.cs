using System.Threading.Tasks;
using Adlg2Helper;
using Microsoft.VisualStudio.TestPlatform.Common.Utilities;
using NUnit.Framework;

namespace Tests.FilesystemClient
{
    public class GetPropertiesTests
    {
        private Adlg2FilesystemClient _client;
        [OneTimeSetUp]
        public async Task Setup()
        {
            _client = Adlg2ClientFactory.BuildFilesystemClient(Configuration.Value("Account"),Configuration.Value("Key"));
            await _client.Create("get-blank-properties");
        }

        [Test]
        public async Task get_blank_properties()
        {
            var properties = await _client.GetProperties("get-blank-properties");
            Assert.IsTrue(string.IsNullOrEmpty(properties.Properties));
        }

        [OneTimeTearDown]
        public async Task Teardown()
        {
            await _client.Delete("get-blank-properties");
        }
    }
}