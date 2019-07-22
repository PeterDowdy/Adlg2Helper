using System.Threading.Tasks;
using Adlg2Helper;
using NUnit.Framework;

namespace Tests.PathClient
{
    public class DeleteTests
    {
        private const string Container = "pathclientdeletetests";
        private Adlg2PathClient _client;
        [OneTimeSetUp]
        public void Setup()
        {
            _client = Adlg2ClientFactory.BuildPathClient(Configuration.Value("Account"),Configuration.Value("Key"));
            foreach (var path in _client.List(Container)) _client.Delete(Container, path.Name, true);
            _client.Create(Container, "existent_file", "file", false);
            _client.Create(Container, "existent_directory", "directory", false);
            _client.Create(Container, "recursive_directory", "directory", false);
            _client.Create(Container, "recursive_directory/level1", "directory", false);
            _client.Create(Container, "recursive_directory_to_fail_to_delete", "directory", false);
            _client.Create(Container, "recursive_directory_to_fail_to_delete/level1", "directory", false);
            _client.Create(Container, "big_directory", "directory", false);
            Parallel.For(0, 5001, x => _client.Create(Container, $"big_directory/{x}", "file", false));
        }

        [Test]
        public void delete_a_file_that_exists()
        {
            Assert.IsTrue(_client.Delete(Container, "existent_file", false));
        }

        [Test]
        public void delete_a_directory_that_exists()
        {
            Assert.IsTrue(_client.Delete(Container, "existent_directory", false));
        }
        [Test]
        public void delete_a_file_that_doesnt_exist()
        {
            Assert.IsFalse(_client.Delete(Container, "nonexistent_file", false));
        }

        [Test]
        public void delete_a_directory_that_doesnt_exist()
        {
            Assert.IsFalse(_client.Delete(Container, "nonexistent_directory", false));
        }

        [Test]
        public void delete_directory_recursively()
        {
            Assert.IsTrue(_client.Delete(Container, "recursive_directory", true));
        }

        [Test]
        public void fail_to_delete_nested_directory_non_recursively()
        {
            Assert.IsFalse(_client.Delete(Container, "recursive_directory_to_fail_to_delete", false));
        }

        [Test]
        public void delete_large_directory()
        {
            Assert.IsTrue(_client.Delete(Container, "big_directory", true));
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            foreach (var path in _client.List(Container)) _client.Delete(Container, path.Name, true);
        }
    }
}