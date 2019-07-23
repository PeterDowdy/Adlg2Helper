using System.Linq;
using Adlg2Helper;
using NUnit.Framework;

namespace Tests.PathClient
{
    public class ListTests
    {
        private const string Container = "pathclientlisttests";
        private Adlg2PathClient _client;
        [OneTimeSetUp]
        public void Setup()
        {
            _client = Adlg2ClientFactory.BuildPathClient(Configuration.Value("Account"),Configuration.Value("Key"));
            Adlg2ClientFactory.BuildFilesystemClient(Configuration.Value("Account"), Configuration.Value("Key")).Create(Container);
            foreach (var path in _client.List(Container)) _client.Delete(Container, path.Name, true);
            _client.Create(Container, "list_test", "directory", false);
            _client.Create(Container, "list_test/list_test_branch_a", "directory", false);
            _client.Create(Container, "list_test/list_test_branch_a/list_test_branch_a_sub_a", "directory", false);
            _client.Create(Container, "list_test/list_test_branch_b", "directory", false);
        }

        [Test]
        public void list_recursively()
        {
            var paths = _client.List(Container, recursive: true);
            Assert.AreEqual(paths.Count(), 4);
        }

        [Test]
        public void list_non_recursively()
        {
            var paths = _client.List(Container, recursive: false);
            Assert.AreEqual(paths.Count(), 1);
        }
        [Test]
        public void list_with_prefix()
        {
            var paths = _client.List(Container, directory: "list_test/list_test_branch_a", recursive: true);
            Assert.AreEqual(paths.Count(), 1);
        }
        [Test]
        public void list_nonexistent()
        {
            var paths = _client.List(Container, directory: "list_test/list_test_branch_c", recursive: true);
            Assert.AreEqual(paths.Count(), 0);
        }

        [Test]
        public void list_with_continuation()
        {
            var paths = _client.List(Container, recursive: true, maxResults: 3);
            Assert.AreEqual(paths.Count(), 4);
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            foreach (var path in _client.List(Container)) _client.Delete(Container, path.Name, true);
        }
    }
}