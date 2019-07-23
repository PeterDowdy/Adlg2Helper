using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Adlg2Helper;
using Microsoft.VisualStudio.TestPlatform.Common.Utilities;
using NUnit.Framework;

namespace Tests.FilesystemClient
{
    public class SetPropertiesTests
    {
        private Adlg2FilesystemClient _client;
        [OneTimeSetUp]
        public void Setup()
        {
            _client = Adlg2ClientFactory.BuildFilesystemClient(Configuration.Value("Account"),Configuration.Value("Key"));
            _client.Create("set-blank-properties");
            _client.Create("set-non-blank-properties");
            _client.Create("set-overwrite-properties");
            while ((_client.GetProperties("set-blank-properties"))?.Properties == null
                   || (_client.GetProperties("set-non-blank-properties"))?.Properties == null
                   || (_client.GetProperties("set-overwrite-properties"))?.Properties == null)
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }

        [Test]
        public void set_blank_properties()
        {
            Assert.IsTrue(_client.SetProperties("set-blank-properties"));
        }

        [Test]
        public void set_non_blank_properties()
        {
            Assert.IsTrue(_client.SetProperties("set-non-blank-properties", new Dictionary<string, string>
            {
                ["test"] = "whoah"
            }));
            var properties = _client.GetProperties("set-non-blank-properties");
            Assert.IsFalse(string.IsNullOrEmpty(properties.Properties));
        }

        [Test]
        public void overwrite_properties()
        {
            Assert.IsTrue(_client.SetProperties("set-overwrite-properties", new Dictionary<string, string>
            {
                ["test"] = "whoah"
            }));
            var properties = _client.GetProperties("set-overwrite-properties");
            Assert.IsFalse(string.IsNullOrEmpty(properties.Properties));
            Assert.IsTrue(_client.SetProperties("set-overwrite-properties", new Dictionary<string, string>
            {
                ["test"] = "dude"
            }));
            properties = _client.GetProperties("set-overwrite-properties");
            Assert.IsFalse(string.IsNullOrEmpty(properties.Properties));
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            _client.Delete("set-blank-properties");
            _client.Delete("set-non-blank-properties");
            _client.Delete("set-overwrite-properties");
        }
    }
}