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
        public void Setup()
        {
            _client = Adlg2ClientFactory.BuildFilesystemClient(Configuration.Value("Account"),Configuration.Value("Key"));
            _client.Create("get-blank-properties");
        }

        [Test]
        public void get_blank_properties()
        {
            var properties = _client.GetProperties("get-blank-properties");
            Assert.IsTrue(string.IsNullOrEmpty(properties.Properties));
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            _client.Delete("get-blank-properties");
        }
    }
}