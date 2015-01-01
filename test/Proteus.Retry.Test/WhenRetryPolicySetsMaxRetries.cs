using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Proteus.Retry.Exceptions;

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
            policy.RetryOnException<ExpectableTestExecption>();

            var testObject = new RetryPolicyMaxRetriesTestSpy();

            var retry = new Retry(policy);

            Assert.Throws<MaxRetryCountReachedException>(() => retry.Invoke(() => testObject.DoWorkThatAlwaysThrows()));
            Assert.That(testObject.InvocationsOfDoWorkThatAlwaysThrows, Is.EqualTo(MAX_RETRIES + 1));
        }

        [Test]
        public void ZeroMaxRetriesStillInvokesDelegateOnce()
        {
            var policy = new RetryPolicy { MaxRetries = 0 };
            policy.RetryOnException<ExpectableTestExecption>();

            var testObject = new RetryPolicyMaxRetriesTestSpy();

            var retry = new Retry(policy);

            Assert.Throws<MaxRetryCountReachedException>(() => retry.Invoke(() => testObject.DoWorkThatAlwaysThrows()));
            Assert.That(testObject.InvocationsOfDoWorkThatAlwaysThrows, Is.EqualTo(1));
        }

        [Test]
        public void WillNotRetryAgainAfterSuccessfulInvocation()
        {
            var policy = new RetryPolicy { MaxRetries = 20 };
            policy.RetryOnException<ExpectableTestExecption>();

            var testObject = new RetryPolicyMaxRetriesTestSpy();

            var retry = new Retry(policy);

            retry.Invoke(() => testObject.DoWorkThatThrowsUntilInvocationCountIs(3));

            Assert.That(testObject.InvocationsOfDoWorkThatThrowsUntil, Is.EqualTo(3));
        }

        [Test]
        public void CannotSetNegativeValueForMaxRetries()
        {
            var policy = new RetryPolicy();
            Assert.Throws<ArgumentOutOfRangeException>(() => policy.MaxRetries = -2);
        }

        [Test]
        public void CanRetrievePastExceptionHistoryOnceMaxRetriesIsReached()
        {
            const int MAX_RETRIES = 20;

            var policy = new RetryPolicy { MaxRetries = MAX_RETRIES };
            policy.RetryOnException<ExpectableTestExecption>();

            var testObject = new RetryPolicyMaxRetriesTestSpy();

            var retry = new Retry(policy);

            try
            {
                retry.Invoke(() => testObject.DoWorkThatAlwaysThrows());
            }
            catch (MaxRetryCountReachedException exception)
            {
                Assert.That(exception.InnerExceptionHistory.Count(), Is.EqualTo(MAX_RETRIES + 1));

                //all exceptions in the list should be of the same pre-canned type based on the test spy hard-coded behavior to always throw
                foreach (var pastException in exception.InnerExceptionHistory)
                {
                    Assert.That(pastException, Is.InstanceOf<ExpectableTestExecption>());
                }
            }
        }

        private class RetryPolicyMaxRetriesTestSpy
        {
            public void DoWorkThatAlwaysThrows()
            {
                InvocationsOfDoWorkThatAlwaysThrows++;
                throw new ExpectableTestExecption();
            }

            public void DoWorkThatThrowsUntilInvocationCountIs(int throwInvocationCount)
            {
                if (InvocationsOfDoWorkThatThrowsUntil < throwInvocationCount)
                {
                    InvocationsOfDoWorkThatThrowsUntil++;
                    throw new ExpectableTestExecption();
                }
            }

            public int InvocationsOfDoWorkThatAlwaysThrows { get; private set; }
            public int InvocationsOfDoWorkThatThrowsUntil { get; private set; }
        }
    }
}
