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
            const int MAX_RETRIES = 3;

            var policy = new RetryPolicy { MaxRetries = MAX_RETRIES };
            policy.AddRetriableException<ExpectableTestExecption>();

            var testObject = new RetryPolicyMaxRetriesTestSpy();

            var retry = new Retry(policy);

            retry.Invoke(() => testObject.DoWorkThatAlwaysThrows());

            Assert.That(testObject.DoWorkThatAlwaysThrowsInvocationCounter, Is.EqualTo(MAX_RETRIES + 1));
        }

        [Test]
        public void ZeroMaxRetriesStillInvokesDelegateOnce()
        {
            var policy = new RetryPolicy { MaxRetries = 0 };
            policy.AddRetriableException<ExpectableTestExecption>();

            var testObject = new RetryPolicyMaxRetriesTestSpy();

            var retry = new Retry(policy);

            retry.Invoke(() => testObject.DoWorkThatAlwaysThrows());

            Assert.That(testObject.DoWorkThatAlwaysThrowsInvocationCounter, Is.EqualTo(1));
        }

        [Test]
        public void WillNotRetryAgainAfterSuccessfulInvocation()
        {
            var policy = new RetryPolicy { MaxRetries = 20 };
            policy.AddRetriableException<ExpectableTestExecption>();

            var testObject = new RetryPolicyMaxRetriesTestSpy();

            var retry = new Retry(policy);

            retry.Invoke(() => testObject.DoWorkThatThrowsUntilInvocationCountIs(3));

            Assert.That(testObject.DoWorkThatThrowsUntilInvocationCounter, Is.EqualTo(3));
        }

        [Test]
        public void CannotSetNegativeValueForMaxRetries()
        {
            var policy = new RetryPolicy();
            Assert.Throws<ArgumentOutOfRangeException>(() => policy.MaxRetries = -2);
        }

        private class RetryPolicyMaxRetriesTestSpy
        {
            public void DoWorkThatAlwaysThrows()
            {
                DoWorkThatAlwaysThrowsInvocationCounter++;
                throw new ExpectableTestExecption();
            }

            public void DoWorkThatThrowsUntilInvocationCountIs(int throwInvocationCount)
            {
                if (DoWorkThatThrowsUntilInvocationCounter < throwInvocationCount)
                {
                    DoWorkThatThrowsUntilInvocationCounter++;
                    throw new ExpectableTestExecption();
                }
            }

            public int DoWorkThatAlwaysThrowsInvocationCounter { get; private set; }
            public int DoWorkThatThrowsUntilInvocationCounter { get; private set; }
        }
    }
}
