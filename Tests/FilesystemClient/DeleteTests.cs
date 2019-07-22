using System.Threading.Tasks;
using Adlg2Helper;
using NUnit.Framework;

namespace Tests.FilesystemClient
{
    public class DeleteTests
    {
        private Adlg2FilesystemClient _client;

        [OneTimeSetUp]
        public async Task Setup()
        {
            _client = Adlg2ClientFactory.BuildFilesystemClient(Configuration.Value("Account"),
                Configuration.Value("Key"));
            await _client.Create("filesystem-to-delete-that-already-exists");
        }

        [Test]
        public async Task delete_a_filesystem()
        {
            Assert.IsTrue(await _client.Delete("filesystem-to-delete-that-already-exists"));
        }

        [Test]
        public async Task delete_a_filesystem_that_doesnt_exist()
        {
            Assert.IsFalse(await _client.Delete("filesystem-to-delete-that-doesnt-exist"));
        }

        [OneTimeTearDown]
        public async Task Teardown()
        {
            await _client.Delete("filesystem-to-delete-that-already-exists");
        }
    }
}