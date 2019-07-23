using Adlg2Helper;
using NUnit.Framework;

namespace Tests.PathClient
{
    public class CreateTests
    {
        private const string Container = "pathclientcreatetests";
        private Adlg2PathClient _client;
        [OneTimeSetUp]
        public void Setup()
        {
            _client = Adlg2ClientFactory.BuildPathClient(Configuration.Value("Account"),Configuration.Value("Key"));
            Adlg2ClientFactory.BuildFilesystemClient(Configuration.Value("Account"), Configuration.Value("Key")).Create(Container);
            foreach (var path in _client.List(Container)) _client.Delete(Container, path.Name, true);
            _client.Create(Container, "existent_file", "file", false);
            _client.Create(Container, "existent_directory", "directory", false);
            _client.Create(Container, "file_to_overwrite", "file", false);
            _client.Create(Container, "directory_to_overwrite", "directory", false);
        }

        [Test]
        public void create_a_file_that_doesnt_exist()
        {
            Assert.IsTrue(_client.Create(Container, "nonexistent_file", "file", false));
        }

        [Test]
        public void create_a_directory_that_doesnt_exist()
        {
            Assert.IsTrue(_client.Create(Container, "nonexistent_directory", "directory", false));
        }
        [Test]
        public void fail_to_create_a_file_that_does_exist()
        {
            Assert.IsFalse(_client.Create(Container, "existent_file", "file", false));
        }

        [Test]
        public void fail_to_create_a_directory_that_does_exist()
        {
            Assert.IsFalse(_client.Create(Container, "existent_directory", "directory", false));
        }
        [Test]
        public void overwrite_a_file()
        {
            Assert.IsTrue(_client.Create(Container, "file_to_overwrite", "file", true));
        }

        [Test]
        public void overwrite_a_directory()
        {
            Assert.IsTrue(_client.Create(Container, "directory_to_overwrite", "directory", true));
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            foreach (var path in _client.List(Container)) _client.Delete(Container, path.Name, true);
        }
    }
}