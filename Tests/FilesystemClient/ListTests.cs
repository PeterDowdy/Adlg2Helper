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
        public async Task Setup()
        {
            _client = Adlg2ClientFactory.BuildFilesystemClient(Configuration.Value("Account"),
                Configuration.Value("Key"));
            await _client.Create("filesystem-to-list");
            await _client.Create("prefix-filesystem-to-list");
        }

        [Test]
        public async Task list_filesystems()
        {
            Assert.IsTrue((await _client.List()).Count(x => x.Name == "filesystem-to-list") == 1);
        }

        [Test]
        public async Task list_filesystems_with_prefix()
        {
            var fileSystems = await _client.List(prefix: "prefix");
            Assert.IsTrue(fileSystems.Count(x => x.Name == "filesystem-to-list") == 0);
            Assert.IsTrue(fileSystems.Count(x => x.Name == "prefix-filesystem-to-list") == 1);
        }


        [OneTimeTearDown]
        public async Task Teardown()
        {
            await _client.Delete("filesystem-to-list");
            await _client.Delete("prefix-filesystem-to-list");
        }
    }
}