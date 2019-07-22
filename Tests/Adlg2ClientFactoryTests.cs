using System;
using System.Collections.Generic;
using System.Text;
using Adlg2Helper;
using NUnit.Framework;

namespace Tests
{
    public class Adlg2ClientFactoryTests
    {
        [Test]
        public void null_accountname()
        {
            Assert.Throws<ArgumentException>(() => Adlg2ClientFactory.BuildPathClient(null, "hi"));
        }
        [Test]
        public void empty_accountname()
        {
            Assert.Throws<ArgumentException>(() => Adlg2ClientFactory.BuildPathClient("", "hi"));
        }
        [Test]
        public void null_key()
        {

            Assert.Throws<ArgumentException>(() => Adlg2ClientFactory.BuildPathClient("hi", null));
        }
        [Test]
        public void empty_key()
        {
            Assert.Throws<ArgumentException>(() => Adlg2ClientFactory.BuildPathClient("hi", ""));
        }
    }
}
