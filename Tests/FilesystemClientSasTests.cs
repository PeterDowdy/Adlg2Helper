using System.Linq;
using Adlg2Helper;
using NUnit.Framework;

namespace Tests
{
    public class FilesystemClientSasTests
    {
        private Adlg2FilesystemClient _client;

        [OneTimeSetUp]
        public void Setup()
        {
            _client = Adlg2ClientFactory.BuildFilesystemClientWithSharedAccessSignature(Configuration.Value("Account"), Configuration.Value("SharedAccessSignature"));
            _client.Create("filesystem-to-list-with-sas");
        }

        [Test]
        public void smoke_test()
        {
            Assert.IsTrue((_client.List()).Count(x => x.Name == "filesystem-to-list-with-sas") == 1);
        }
    }
}