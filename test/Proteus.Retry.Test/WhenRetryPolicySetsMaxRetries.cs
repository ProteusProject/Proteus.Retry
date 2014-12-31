using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Proteus.Retry.Test
{
    [TestFixture]
    public class WhenRetryPolicySetsMaxRetries
    {
        [Test]
        public void MaxRetriesAreRespected()
        {
            var policy = new RetryPolicy {MaxRetries = 3};

            var testObject = new RetryPolicyMaxRetriesTestSpy();

            var retry = new Retry(policy);

            retry.Invoke(() => testObject.DoWorkThatThrows());

            Assert.That(testObject.RetryCounter, Is.EqualTo(3));
        }
    }

    public class RetryPolicyMaxRetriesTestSpy
    {
        public void DoWorkThatThrows()
        {
            RetryCounter++;
            throw new NotImplementedException();
        }

        public int RetryCounter { get; private set; }
    }
}
