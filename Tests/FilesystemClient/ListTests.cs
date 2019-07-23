using System.Linq;
using System.Threading.Tasks;
using Adlg2Helper;
using NUnit.Framework;

namespace Tests.FilesystemClient
{
    public class ListTests
    {
        private Adlg2FilesystemClient _client;

        [OneTimeSetUp]
        public void Setup()
        {
            _client = Adlg2ClientFactory.BuildFilesystemClient(Configuration.Value("Account"),
                Configuration.Value("Key"));
            _client.Create("filesystem-to-list");
            _client.Create("prefix-filesystem-to-list");
        }

        [Test]
        public void list_filesystems()
        {
            Assert.IsTrue((_client.List()).Count(x => x.Name == "filesystem-to-list") == 1);
        }

        [Test]
        public void list_filesystems_with_prefix()
        {
            var fileSystems = _client.List(prefix: "prefix");
            Assert.IsTrue(fileSystems.Count(x => x.Name == "filesystem-to-list") == 0);
            Assert.IsTrue(fileSystems.Count(x => x.Name == "prefix-filesystem-to-list") == 1);
        }


        [OneTimeTearDown]
        public void Teardown()
        {
            _client.Delete("filesystem-to-list");
            _client.Delete("prefix-filesystem-to-list");
        }
    }
}