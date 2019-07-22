using System.IO;
using System.Linq;
using Adlg2Helper;
using NUnit.Framework;

namespace Tests.PathClient
{
    public class ReadTests
    {
        private const string Container = "pathclientreadtests";
        private Adlg2PathClient _client;
        [OneTimeSetUp]
        public void Setup()
        {
            _client = Adlg2ClientFactory.BuildPathClient(Configuration.Value("Account"),Configuration.Value("Key"));
            foreach (var path in _client.List(Container)) _client.Delete(Container, path.Name, true);
            _client.Create(Container, "small_read_file", "file", false);
            _client.Update(Container, "small_read_file", "append", Enumerable.Range(0,998).Select(x => (byte)(x % 255)).ToArray());
            _client.Update(Container, "small_read_file", "flush", position:998);
            _client.Create(Container, "large_read_file", "file", false);
            _client.Update(Container, "large_read_file", "append", Enumerable.Range(0, 2405342).Select(x => (byte)(x % 255)).ToArray());
            _client.Update(Container, "large_read_file", "flush", position: 2405342);
        }

        [Test]
        public void read_small_file_as_stream()
        {
            using (var file = _client.ReadStream(Container, "small_read_file", 0, 998))
            {
                Assert.AreEqual(file.Length, 998);
            }
        }
        [Test]
        public void read_small_file_as_byte_array()
        {
            var file = _client.ReadBytes(Container, "small_read_file", 0, 998);
            Assert.AreEqual(file.Length, 998);
        }

        [Test]
        public void read_large_file_as_stream()
        {
            using (var file = _client.ReadStream(Container, "large_read_file", 0, 2405342))
            {
                Assert.AreEqual(file.Length, 2405342);
            }
        }

        [Test]
        public void read_large_file_as_byte_array()
        {
            var file = _client.ReadBytes(Container, "large_read_file", 0, 2405342);
            Assert.AreEqual(file.Length, 2405342);
        }

        [Test]
        public void read_large_file_as_stream_in_two_parts()
        {
            using (var file1 = _client.ReadStream(Container, "large_read_file", 0, 2405342 / 2))
            using (var file2 = _client.ReadStream(Container, "large_read_file", 1 + 2405342 / 2, 2405342))
            {
                Assert.AreEqual(file1.Length + file2.Length, 2405342);
            }
        }

        [Test]
        public void read_large_file_as_byte_array_in_two_parts()
        {
            var file1 = _client.ReadBytes(Container, "large_read_file", 0, 2405342 / 2);
            var file2 = _client.ReadBytes(Container, "large_read_file", 1 + 2405342 / 2, 2405342);
            Assert.AreEqual(file1.Length + file2.Length, 2405342);
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            foreach (var path in _client.List(Container)) _client.Delete(Container, path.Name, true);
        }
    }
}