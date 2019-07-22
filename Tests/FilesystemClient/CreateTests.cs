using System.Threading.Tasks;
using Adlg2Helper;
using NUnit.Framework;

namespace Tests.FilesystemClient
{
    public class CreateTests
    {
        private Adlg2FilesystemClient _client;

        [OneTimeSetUp]
        public async Task Setup()
        {
            _client = Adlg2ClientFactory.BuildFilesystemClient(Configuration.Value("Account"),
                Configuration.Value("Key"));
            await _client.Create("filesystem-to-create-that-already-exists");
        }

        [Test]
        public async Task create_a_filesystem()
        {
            Assert.IsTrue(await _client.Create("filesystem-to-create"));
        }

        [Test]
        public async Task create_a_filesystem_that_exists()
        {
            Assert.IsFalse(await _client.Create("filesystem-to-create-that-already-exists"));
        }

        [OneTimeTearDown]
        public async Task Teardown()
        {
            await _client.Delete("filesystem-to-create");
            await _client.Delete("filesystem-to-create-that-already-exists");
        }
    }
}