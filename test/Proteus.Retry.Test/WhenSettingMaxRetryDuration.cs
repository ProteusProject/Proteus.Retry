﻿using System;
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

            var policy = new RetryPolicy();
            policy.MaxRetryDuration = TimeSpan.FromSeconds(5);
            policy.MaxRetries = MAX_RETRIES;
            policy.RegisterRetriableException<ExpectableTestExecption>();

            var retry = new Retry(policy);
            
            var instance = new MaxRetryDurationTestSpy();

            Assert.Throws<MaxRetryDurationExpiredException>(() => retry.Invoke(() => instance.MethodThatSleepsThenAlwaysThrows(1000)));

            //cannot assert a specific value here b/c the isn't sufficiently fine-grained for consistent predictable results
            // (should be ~5 or 6 here, but *GUARANTEED* to be well-short of the MAX_RETRIES value)
            Assert.That(instance.SleepMethodInvocations, Is.LessThan(MAX_RETRIES));
        }


        private class MaxRetryDurationTestSpy
        {
            public void MethodThatSleepsThenAlwaysThrows(int sleepMilliseconds)
            {
                SleepMethodInvocations++;
                Thread.Sleep(sleepMilliseconds);
                throw new ExpectableTestExecption();
            }

            public int SleepMethodInvocations { get; private set; }
        }
    }
}