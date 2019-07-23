using System;
using System.Collections.Generic;
using System.Threading;
using Adlg2Helper;
using NUnit.Framework;

namespace Tests.PathClient
{
    public class LeaseTests
    {
        private const string Container = "pathclientleasetests";
        private Adlg2PathClient _client;
        [OneTimeSetUp]
        public void Setup()
        {
            _client = Adlg2ClientFactory.BuildPathClient(Configuration.Value("Account"),Configuration.Value("Key"));
            Adlg2ClientFactory.BuildFilesystemClient(Configuration.Value("Account"), Configuration.Value("Key")).Create(Container);
            foreach (var path in _client.List(Container)) _client.Delete(Container, path.Name, true);
            _client.Create(Container, "acquire_a_lease", "file", false);
            _client.Create(Container, "acquire_a_lease_on_a_file_that_is_already_leased", "file", false);
            _client.Create(Container, "acquire_a_lease_to_be_broken", "file", false);
            _client.Create(Container, "expired_lease_to_be_broken", "file", false);
            _client.Create(Container, "break_a_lease_twice", "file", false);
            _client.Create(Container, "nonexistent_lease", "file", false);
            _client.Create(Container, "change_a_lease", "file", false);
            _client.Create(Container, "change_an_expired_lease", "file", false);
            _client.Create(Container, "change_a_nonexistent_lease", "file", false);
            _client.Create(Container, "renew_a_lease", "file", false);
            _client.Create(Container, "renew_an_expired_lease", "file", false);
            _client.Create(Container, "renew_a_nonexistent_lease", "file", false);
            _client.Create(Container, "renew_broken_lease", "file", false);
            _client.Create(Container, "release_a_lease", "file", false);
            _client.Create(Container, "release_an_expired_lease", "file", false);
            _client.Create(Container, "release_a_nonexistent_lease", "file", false);
        }

        [Test]
        public void acquire_a_lease()
        {
            var desiredLeaseId = Guid.NewGuid().ToString();
            Assert.IsTrue(_client.Lease(Container, "acquire_a_lease", "acquire", proposedLeaseId: desiredLeaseId, returnedLeaseId:out var actualLeaseId, leaseDuration: 15));
            Assert.AreEqual(actualLeaseId, desiredLeaseId);
        }

        [Test]
        public void acquire_a_lease_on_a_nonexistent_file()
        {
            var desiredLeaseId = Guid.NewGuid().ToString();
            Assert.IsFalse(_client.Lease(Container, "acquire_a_lease_on_a_nonexistent_file", "acquire", proposedLeaseId: desiredLeaseId, returnedLeaseId: out var actualLeaseId, leaseDuration: 15));
        }

        [Test]
        public void acquire_a_lease_on_a_file_that_is_already_leased()
        {
            _client.Lease(Container, "acquire_a_lease_on_a_file_that_is_already_leased", "acquire", proposedLeaseId: Guid.NewGuid().ToString(), returnedLeaseId: out _, leaseDuration: 15);
            Assert.IsFalse(_client.Lease(Container, "acquire_a_lease_on_a_file_that_is_already_leased", "acquire", proposedLeaseId: Guid.NewGuid().ToString(), returnedLeaseId: out _, leaseDuration: 15));
        }

        [Test]
        public void break_a_lease()
        {
            var desiredLeaseId = Guid.NewGuid().ToString();
            Assert.IsTrue(_client.Lease(Container, "acquire_a_lease_to_be_broken", "acquire", proposedLeaseId: desiredLeaseId, returnedLeaseId: out _, leaseDuration: 15));
            Assert.IsTrue(_client.Lease(Container, "acquire_a_lease_to_be_broken", "break", returnedLeaseId: out _));
        }

        [Test]
        public void break_an_expired_lease()
        {
            var desiredLeaseId = Guid.NewGuid().ToString();
            Assert.IsTrue(_client.Lease(Container, "expired_lease_to_be_broken", "acquire", proposedLeaseId: desiredLeaseId, returnedLeaseId: out _, leaseDuration: 15));
            Thread.Sleep(16 * 1000);
            Assert.IsTrue(_client.Lease(Container, "expired_lease_to_be_broken", "break", returnedLeaseId: out _));
        }

        [Test]
        public void break_nonexistent_lease()
        {
            Assert.IsFalse(_client.Lease(Container, "nonexistent_lease", "break", returnedLeaseId: out _));
        }

        [Test]
        public void break_a_lease_twice()
        {
            var desiredLeaseId = Guid.NewGuid().ToString();
            Assert.IsTrue(_client.Lease(Container, "break_a_lease_twice", "acquire", proposedLeaseId: desiredLeaseId, returnedLeaseId: out _, leaseDuration: 15));
            Assert.IsTrue(_client.Lease(Container, "break_a_lease_twice", "break", returnedLeaseId: out _));
            Assert.IsTrue(_client.Lease(Container, "break_a_lease_twice", "break", returnedLeaseId: out _));
        }

        [Test]
        public void change_a_lease()
        {
            var firstLeaseId = Guid.NewGuid().ToString();
            Assert.IsTrue(_client.Lease(Container, "change_a_lease", "acquire", proposedLeaseId: firstLeaseId, returnedLeaseId: out _, leaseDuration: 15));
            var newLeaseId = Guid.NewGuid().ToString();
            Assert.IsTrue(_client.Lease(Container, "change_a_lease", "change", proposedLeaseId: newLeaseId, returnedLeaseId: out var returnedLeaseId, leaseId: firstLeaseId));
            Assert.AreEqual(newLeaseId, returnedLeaseId);
        }

        [Test]
        public void change_an_expired_lease()
        {
            var firstLeaseId = Guid.NewGuid().ToString();
            Assert.IsTrue(_client.Lease(Container, "change_an_expired_lease", "acquire", proposedLeaseId: firstLeaseId, returnedLeaseId: out _, leaseDuration: 15));
            Thread.Sleep(16*1000);
            var newLeaseId = Guid.NewGuid().ToString();
            Assert.IsFalse(_client.Lease(Container, "change_an_expired_lease", "change", proposedLeaseId: newLeaseId, returnedLeaseId: out var returnedLeaseId, leaseId: firstLeaseId));
        }

        [Test]
        public void change_a_nonexistent_lease()
        {
            var firstLeaseId = Guid.NewGuid().ToString();
            Assert.IsTrue(_client.Lease(Container, "change_a_nonexistent_lease", "acquire", proposedLeaseId: firstLeaseId, returnedLeaseId: out _, leaseDuration: 15));
            var newLeaseId = Guid.NewGuid().ToString();
            Assert.IsFalse(_client.Lease(Container, "change_a_nonexistent_lease", "change", proposedLeaseId: newLeaseId, returnedLeaseId: out var returnedLeaseId, leaseId: Guid.NewGuid().ToString()));
        }
        [Test]
        public void renew_a_lease()
        {
            var firstLeaseId = Guid.NewGuid().ToString();
            Assert.IsTrue(_client.Lease(Container, "renew_a_lease", "acquire", proposedLeaseId: firstLeaseId, returnedLeaseId: out _, leaseDuration: 15));
            Assert.IsTrue(_client.Lease(Container, "renew_a_lease", "renew", returnedLeaseId: out var _, leaseId: firstLeaseId));
        }

        [Test]
        public void renew_an_expired_lease()
        {
            var firstLeaseId = Guid.NewGuid().ToString();
            Assert.IsTrue(_client.Lease(Container, "renew_an_expired_lease", "acquire", proposedLeaseId: firstLeaseId, returnedLeaseId: out _, leaseDuration: 15));
            Thread.Sleep(16 * 1000);
            Assert.IsTrue(_client.Lease(Container, "renew_an_expired_lease", "renew", returnedLeaseId: out _, leaseId: firstLeaseId));
        }

        [Test]
        public void renew_nonexistent_lease()
        {
            var firstLeaseId = Guid.NewGuid().ToString();
            Assert.IsTrue(_client.Lease(Container, "renew_a_nonexistent_lease", "acquire", proposedLeaseId: firstLeaseId, returnedLeaseId: out _, leaseDuration: 15));
            var newLeaseId = Guid.NewGuid().ToString();
            Assert.IsFalse(_client.Lease(Container, "renew_a_nonexistent_lease", "renew", returnedLeaseId: out _, leaseId: Guid.NewGuid().ToString()));
        }

        [Test]
        public void renew_broken_lease()
        {
            var firstLeaseId = Guid.NewGuid().ToString();
            Assert.IsTrue(_client.Lease(Container, "renew_broken_lease", "acquire", proposedLeaseId: firstLeaseId, returnedLeaseId: out _, leaseDuration: 15));
            Thread.Sleep(1000);
            Assert.IsTrue(_client.Lease(Container, "renew_broken_lease", "break", returnedLeaseId: out _, leaseBreakPeriod: 0));
            Thread.Sleep(1000);
            Assert.IsFalse(_client.Lease(Container, "renew_broken_lease", "renew", returnedLeaseId: out _, leaseId: firstLeaseId));
        }

        [Test]
        public void release_a_lease()
        {
            var firstLeaseId = Guid.NewGuid().ToString();
            Assert.IsTrue(_client.Lease(Container, "release_a_lease", "acquire", proposedLeaseId: firstLeaseId, returnedLeaseId: out _, leaseDuration: 15));
            Assert.IsTrue(_client.Lease(Container, "release_a_lease", "release", returnedLeaseId: out _, leaseId: firstLeaseId));
        }

        [Test]
        public void release_an_expired_lease()
        {
            var firstLeaseId = Guid.NewGuid().ToString();
            Assert.IsTrue(_client.Lease(Container, "release_an_expired_lease", "acquire", proposedLeaseId: firstLeaseId, returnedLeaseId: out _, leaseDuration: 15));
            Thread.Sleep(16 * 1000);
            Assert.IsTrue(_client.Lease(Container, "release_an_expired_lease", "release", returnedLeaseId: out _, leaseId: firstLeaseId));
        }

        [Test]
        public void release_a_nonexistent_lease()
        {
            var firstLeaseId = Guid.NewGuid().ToString();
            Assert.IsTrue(_client.Lease(Container, "release_a_nonexistent_lease", "acquire", proposedLeaseId: firstLeaseId, returnedLeaseId: out _, leaseDuration: 15));
            var newLeaseId = Guid.NewGuid().ToString();
            Assert.IsFalse(_client.Lease(Container, "release_a_nonexistent_lease", "release", returnedLeaseId: out _, leaseId: Guid.NewGuid().ToString()));
        }
    }
}