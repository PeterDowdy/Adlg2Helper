using Adlg2Helper;
using NUnit.Framework;

namespace Tests.PathClient
{
    public class UpdateTests
    {
        private const string Container = "pathclientupdatetests";
        private Adlg2PathClient _client;
        [OneTimeSetUp]
        public void Setup()
        {
            _client = Adlg2ClientFactory.BuildPathClient("adlsg2clienttests", "R64aecLatzW8RO347vRqNBpwFkjTbUiiQmNmKXDpDe3NzLmo8n4uahtmcj4o+6W7VTZrgl6Q5l3SzB7U/R8QDA==");
            foreach (var path in _client.List(Container)) _client.Delete(Container, path.Name, true);
            _client.Create(Container, "file_to_upload_to", "file", false);
            _client.Create(Container, "file_to_flush_to", "file", false);
            _client.Create(Container, "file_to_append_and_flush_to", "file", false);
        }

        [Test]
        public void upload_data_to_file()
        {
            Assert.IsTrue(_client.Update(Container, "file_to_upload_to", "append", new byte[] {1,2,3,4}));
        }

        [Test]
        public void flush_data_to_file()
        {
            _client.Update(Container, "file_to_flush_to", "append", new byte[] { 1, 2, 3, 4 });
            Assert.IsTrue(_client.Update(Container, "file_to_flush_to", "flush", null,4,close: true));
        }

        [Test]
        public void append_and_flush()
        {
            _client.Update(Container, "file_to_append_and_flush_to", "append", new byte[] { 1, 2, 3, 4 });
            _client.Update(Container, "file_to_append_and_flush_to", "append", new byte[] { 5,6,7,8 }, position:4);
            Assert.IsTrue(_client.Update(Container, "file_to_append_and_flush_to", "flush", null, 8, close: true));
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            foreach (var path in _client.List(Container)) _client.Delete(Container, path.Name, true);
        }
    }
}