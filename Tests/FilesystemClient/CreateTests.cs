using System.Threading.Tasks;
using Adlg2Helper;
using NUnit.Framework;

namespace Tests.FilesystemClient
{
    public class CreateTests
    {
        private Adlg2FilesystemClient _client;

        [OneTimeSetUp]
        public void Setup()
        {
            _client = Adlg2ClientFactory.BuildFilesystemClient(Configuration.Value("Account"), Configuration.Value("Key"));
            _client.Create("filesystem-to-create-that-already-exists");
        }

        [Test]
        public void create_a_filesystem()
        {
            Assert.IsTrue(_client.Create("filesystem-to-create"));
        }

        [Test]
        public void create_a_filesystem_that_exists()
        {
            Assert.IsFalse(_client.Create("filesystem-to-create-that-already-exists"));
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            _client.Delete("filesystem-to-create");
            _client.Delete("filesystem-to-create-that-already-exists");
        }
    }
}