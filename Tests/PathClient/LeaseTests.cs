using System;
using System.Collections.Generic;
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
            _client = Adlg2ClientFactory.BuildPathClient("adlsg2clienttests", "R64aecLatzW8RO347vRqNBpwFkjTbUiiQmNmKXDpDe3NzLmo8n4uahtmcj4o+6W7VTZrgl6Q5l3SzB7U/R8QDA==");
            foreach (var path in _client.List(Container)) _client.Delete(Container, path.Name, true);
            _client.Create(Container, "acquire_a_lease", "file", false);
            _client.Create(Container, "acquire_a_lease_on_a_file_that_is_already_leased", "file", false);
            _client.Create(Container, "acquire_a_lease_to_be_broken", "file", false);
        }

        [Test]
        public void acquire_a_lease()
        {
            var desiredLeaseId = Guid.NewGuid().ToString();
            Assert.IsTrue(_client.Lease(Container, "acquire_a_lease", "acquire", leaseId: desiredLeaseId, returnedLeaseId:out var actualLeaseId, leaseDuration: 15));
            Assert.AreEqual(actualLeaseId, desiredLeaseId);
        }

        [Test]
        public void acquire_a_lease_on_a_nonexistent_file()
        {
            var desiredLeaseId = Guid.NewGuid().ToString();
            Assert.IsFalse(_client.Lease(Container, "acquire_a_lease_on_a_nonexistent_file", "acquire", leaseId: desiredLeaseId, returnedLeaseId: out var actualLeaseId, leaseDuration: 15));
        }

        [Test]
        public void acquire_a_lease_on_a_file_that_is_already_leased()
        {
            _client.Lease(Container, "acquire_a_lease_on_a_file_that_is_already_leased", "acquire", leaseId: Guid.NewGuid().ToString(), returnedLeaseId: out _, leaseDuration: 15);
            Assert.IsFalse(_client.Lease(Container, "acquire_a_lease_on_a_file_that_is_already_leased", "acquire", leaseId: Guid.NewGuid().ToString(), returnedLeaseId: out _, leaseDuration: 15));
        }

        [Test]
        public void break_a_lease()
        {
            var desiredLeaseId = Guid.NewGuid().ToString();
            Assert.IsTrue(_client.Lease(Container, "acquire_a_lease_to_be_broken", "acquire", leaseId: desiredLeaseId, returnedLeaseId: out _, leaseDuration: 15));
            Assert.IsTrue(_client.Lease(Container, "acquire_a_lease_to_be_broken", "break", returnedLeaseId: out _));
        }

        [Test]
        public void break_an_expired_lease()
        {
            Assert.Fail();
        }

        [Test]
        public void break_nonexistent_lease()
        {
            Assert.Fail();
        }

        [Test]
        public void change_a_lease()
        {
            Assert.Fail();
        }

        [Test]
        public void change_an_expired_lease()
        {
            Assert.Fail();
        }

        [Test]
        public void change_a_nonexistent_lease()
        {
            Assert.Fail();
        }
        [Test]
        public void renew_a_lease()
        {
            Assert.Fail();
        }

        [Test]
        public void renew_an_expired_lease()
        {
            Assert.Fail();
        }

        [Test]
        public void renew_nonexistent_lease()
        {
            Assert.Fail();
        }

        [Test]
        public void release_a_lease()
        {
            Assert.Fail();
        }

        [Test]
        public void release_an_expired_lease()
        {
            Assert.Fail();
        }

        [Test]
        public void release_a_nonexistent_lease()
        {
            Assert.Fail();
        }
    }
}