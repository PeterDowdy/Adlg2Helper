using System.Linq;
using Adlg2Helper;
using NUnit.Framework;

namespace Tests
{
    public class FilesystemClientOauthTests
    {
        private Adlg2FilesystemClient _client;

        [OneTimeSetUp]
        public void Setup()
        {
            _client = Adlg2ClientFactory.BuildFilesystemClient(Configuration.Value("Account"), Configuration.Value("TenantId"), Configuration.Value("ClientId"), Configuration.Value("ClientSecret"));
            _client.Create("filesystem-to-list-with-oauth");
        }

        [Test]
        public void smoke_test()
        {
            Assert.IsTrue((_client.List()).Count(x => x.Name == "filesystem-to-list-with-oauth") == 1);
        }
    }
}