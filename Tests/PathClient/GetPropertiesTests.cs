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
            foreach (var path in _client.List(Container)) _client.Delete(Container, path.Name, true);
            _client.Create(Container, "get_properties", "file", false);
        }

        [Test]
        public async Task get_properties()
        {
            var properties = await _client.GetProperties(Container, "get_properties");
            Assert.AreEqual(properties.Owner, $"$superuser");
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            foreach (var path in _client.List(Container)) _client.Delete(Container, path.Name, true);
        }
    }
}