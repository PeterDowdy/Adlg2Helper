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
            _client = Adlg2ClientFactory.BuildPathClient("adlsg2clienttests", "R64aecLatzW8RO347vRqNBpwFkjTbUiiQmNmKXDpDe3NzLmo8n4uahtmcj4o+6W7VTZrgl6Q5l3SzB7U/R8QDA==");
            foreach (var path in _client.List(Container)) _client.Delete(Container, path.Name, true);
        }

        [Test]
        public void Test1()
        {
            Assert.Fail();
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            foreach (var path in _client.List(Container)) _client.Delete(Container, path.Name, true);
        }
    }
}