using System;
using System.Linq;
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
            _client = Adlg2ClientFactory.BuildPathClient(Configuration.Value("Account"),Configuration.Value("Key"));
            foreach (var path in _client.List(Container)) _client.Delete(Container, path.Name, true);
            _client.Create(Container, "file_to_upload_to", "file", false);
            _client.Create(Container, "file_to_flush_to", "file", false);
            _client.Create(Container, "file_to_append_and_flush_to", "file", false);
            _client.Create(Container, "file_greater_than_4mb", "file", false);
            _client.Create(Container, "file_to_append_and_flush_to_out_of_order", "file", false);
            _client.Create(Container, "verify_content", "file", false);
            _client.Create(Container, "invalid_flush", "file", false);
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

        [Test]
        public void write_large_file()
        {
            Assert.IsTrue(_client.Update(Container, "file_greater_than_4mb", "append", Enumerable.Range(0,16*1024*1024).Select(x => (byte)(x%255)).ToArray()));
            Assert.IsTrue(_client.Update(Container, "file_greater_than_4mb", "flush", null, 16 * 1024 * 1024, close: true));
        }

        [Test]
        public void append_and_flush_out_of_order()
        {
            _client.Update(Container, "file_to_append_and_flush_to_out_of_order", "append", new byte[] { 5, 6, 7, 8 }, position: 4);
            _client.Update(Container, "file_to_append_and_flush_to_out_of_order", "append", new byte[] { 1, 2, 3, 4 });
            Assert.IsTrue(_client.Update(Container, "file_to_append_and_flush_to_out_of_order", "flush", null, 8, close: true));
        }

        [Test]
        public void verify_content()
        {
            _client.Update(Container, "verify_content", "append", new byte[] { 1, 2, 3, 4 });
            Assert.IsTrue(_client.Update(Container, "verify_content", "flush", null, 4, close: true));
            Assert.AreEqual(_client.ReadBytes(Container, "verify_content", 0, 4).Length, 4);
        }
        [Test]
        public void cant_flush_without_a_position()
        {
            _client.Update(Container, "verify_content", "append", new byte[] { 1, 2, 3, 4 });
            Assert.Throws<ArgumentException>(() => _client.Update(Container, "verify_content", "flush", null, null, close: true));
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            foreach (var path in _client.List(Container)) _client.Delete(Container, path.Name, true);
        }
    }
}