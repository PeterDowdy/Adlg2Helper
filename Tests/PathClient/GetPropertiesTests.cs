using System.Threading.Tasks;
using Adlg2Helper;
using NUnit.Framework;

namespace Tests.PathClient
{
    public class GetPropertiesTests
    {
        private const string Container = "pathclientgetpropertiestests";
        private Adlg2PathClient _client;
        [OneTimeSetUp]
        public void Setup()
        {
            _client = Adlg2ClientFactory.BuildPathClient(Configuration.Value("Account"),Configuration.Value("Key"));
            Adlg2ClientFactory.BuildFilesystemClient(Configuration.Value("Account"), Configuration.Value("Key")).Create(Container);
            foreach (var path in _client.List(Container)) _client.Delete(Container, path.Name, true);
            _client.Create(Container, "get_properties", "file", false);
        }

        [Test]
        public void get_properties()
        {
            var properties = _client.GetProperties(Container, "get_properties");
            Assert.AreEqual(properties.Owner, $"$superuser");
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            foreach (var path in _client.List(Container)) _client.Delete(Container, path.Name, true);
        }
    }
}