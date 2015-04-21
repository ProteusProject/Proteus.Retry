using System;
using System.Configuration;
using System.Threading;
using NUnit.Framework;
using Proteus.Retry.Exceptions;

namespace Proteus.Retry.Test
{
    [TestFixture]
    public class WhenSettingMaxRetryDuration
    {
        [Test]
        public void RetriesAreCanceledOnceDurationExpires()
        {
            const int MAX_RETRIES = 20;
            const int MAX_RETRY_DURATION = 5000;
            const int SINGLE_INVOCATION_SLEEP_DURATION = 1000;

            Assume.That(MAX_RETRY_DURATION < MAX_RETRIES * SINGLE_INVOCATION_SLEEP_DURATION);

            var policy = new RetryPolicy();
            policy.MaxRetryDuration = TimeSpan.FromMilliseconds(MAX_RETRY_DURATION);
            policy.MaxRetries = MAX_RETRIES;
            policy.RegisterRetriableException<ExpectableTestExecption>();

            var retry = new Retry(policy);
            
            var instance = new MaxRetryDurationTestSpy();

            Assert.Throws<MaxRetryDurationExpiredException>(() => retry.Invoke(() => instance.MethodThatSleepsThenAlwaysThrowsAfter(SINGLE_INVOCATION_SLEEP_DURATION)));

            //cannot assert a specific value here b/c the timer isn't sufficiently fine-grained for consistent predictable results
            // (should be ~5 or 6 here, but *GUARANTEED* to be well-short of the MAX_RETRIES value)
            Assert.That(instance.SleepMethodInvocations, Is.LessThan(MAX_RETRIES));
        }


        private class MaxRetryDurationTestSpy
        {
            public void MethodThatSleepsThenAlwaysThrowsAfter(int sleepMilliseconds)
            {
                SleepMethodInvocations++;
                Thread.Sleep(sleepMilliseconds);
                throw new ExpectableTestExecption();
            }

            public int SleepMethodInvocations { get; private set; }
        }
    }
}