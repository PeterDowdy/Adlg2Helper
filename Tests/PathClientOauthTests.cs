using System.Linq;
using Adlg2Helper;
using NUnit.Framework;

namespace Tests
{
    public class PathClientOauthTests
    {
        private const string Container = "pathclientlisttests";
        private Adlg2PathClient _client;
        [OneTimeSetUp]
        public void Setup()
        {
            _client = Adlg2ClientFactory.BuildPathClient(Configuration.Value("Account"), Configuration.Value("TenantId"),Configuration.Value("ClientId"),Configuration.Value("ClientSecret"));
            Adlg2ClientFactory.BuildFilesystemClient(Configuration.Value("Account"), Configuration.Value("TenantId"), Configuration.Value("ClientId"), Configuration.Value("ClientSecret")).Create(Container);
            foreach (var path in _client.List(Container)) _client.Delete(Container, path.Name, true);
            _client.Create(Container, "list_test_with_oauth", "directory", false);
        }

        [Test]
        public void smoke_test()
        {
            var paths = _client.List(Container, recursive: true);
            Assert.AreEqual(paths.Count(), 1);
        }
        [OneTimeTearDown]
        public void Teardown()
        {
            foreach (var path in _client.List(Container)) _client.Delete(Container, path.Name, true);
        }
    }
}