using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using NUnit.Framework;
using Proteus.Retry.Exceptions;

namespace Proteus.Retry.Test
{
    [TestFixture]
    public class WhenSettingMaxRetries
    {
        [Test]
        public void DefaultMaxRetriesIsZero()
        {
            var policy = new RetryPolicy();
            Assert.That(policy.MaxRetries, Is.EqualTo(0));
        }
        
        [Test]
        public void MaxRetriesAreRespected()
        {
            const int MAX_RETRIES = 3;

            var policy = new RetryPolicy { MaxRetries = MAX_RETRIES };
            policy.RegisterRetriableException<ExpectableTestExecption>();

            var testObject = new RetryPolicyMaxRetriesTestSpy();

            var retry = new Retry(policy);
            retry.Logger = LogManager.GetLogger(this.GetType());

            Assert.Throws<MaxRetryCountExceededException>(() => retry.Invoke(() => testObject.DoWorkThatAlwaysThrows()));
            Assert.That(testObject.InvocationsOfDoWorkThatAlwaysThrows, Is.EqualTo(MAX_RETRIES + 1));
        }

        [Test]
        public void ZeroMaxRetriesStillInvokesDelegateOnce()
        {
            var policy = new RetryPolicy { MaxRetries = 0 };
            policy.RegisterRetriableException<ExpectableTestExecption>();

            var testObject = new RetryPolicyMaxRetriesTestSpy();

            var retry = new Retry(policy);
            retry.Logger = LogManager.GetLogger(this.GetType());

            Assert.Throws<MaxRetryCountExceededException>(() => retry.Invoke(() => testObject.DoWorkThatAlwaysThrows()));
            Assert.That(testObject.InvocationsOfDoWorkThatAlwaysThrows, Is.EqualTo(1));
        }

        [Test]
        public void WillNotRetryAgainAfterSuccessfulInvocation()
        {
            const int MAX_RETRIES = 20;
            const int THROW_UNTIL = 3;

            Assume.That(MAX_RETRIES > THROW_UNTIL);


            var policy = new RetryPolicy { MaxRetries = MAX_RETRIES };
            policy.RegisterRetriableException<ExpectableTestExecption>();

            var testObject = new RetryPolicyMaxRetriesTestSpy();

            var retry = new Retry(policy);
            retry.Logger = LogManager.GetLogger(this.GetType());

            retry.Invoke(() => testObject.DoWorkThatThrowsUntilInvocationCountIs(THROW_UNTIL));

            Assert.That(testObject.InvocationsOfDoWorkThatThrowsUntil, Is.EqualTo(THROW_UNTIL));
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
            policy.RegisterRetriableException<ExpectableTestExecption>();

            var testObject = new RetryPolicyMaxRetriesTestSpy();

            var retry = new Retry(policy);
            retry.Logger = LogManager.GetLogger(this.GetType());

            try
            {
                retry.Invoke(() => testObject.DoWorkThatAlwaysThrows());
            }
            catch (MaxRetryCountExceededException exception)
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
