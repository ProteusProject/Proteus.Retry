#region License

/*
 * Copyright © 2014-2016 the original author or authors.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System;
using System.Threading;
using Common.Logging;
using NUnit.Framework;
using Proteus.Retry.Exceptions;

namespace Proteus.Retry.Test
{
    [TestFixture]
    public class WhenSettingMaxRetryDuration
    {

        [Test]
        public void HasMaxRetryDurationReportsIfSet()
        {
            var policy = new RetryPolicy { MaxRetryDuration = TimeSpan.FromMilliseconds(10) };
            Assert.That(policy.HasMaxRetryDuration, Is.True);
        }

        [Test]
        public void HasMaxRetryDurationReportsIfNotSet()
        {
            var policy = new RetryPolicy();
            Assert.That(policy.HasMaxRetryDuration, Is.False);
        }

        [Test]
        public void RetriesAreCanceledOnceDurationExpires()
        {
            const int MAX_RETRIES = 20;
            const int MAX_RETRY_DURATION = 5000;
            const int SINGLE_INVOCATION_SLEEP_DURATION = 1000;

            Assume.That(MAX_RETRY_DURATION < MAX_RETRIES * SINGLE_INVOCATION_SLEEP_DURATION);

            var policy = new RetryPolicy
            {
                MaxRetryDuration = TimeSpan.FromMilliseconds(MAX_RETRY_DURATION),
                MaxRetries = MAX_RETRIES
            };
            policy.RegisterRetriableException<ExpectableTestException>();

            var retry = new Retry(policy) { Logger = msg => LogManager.GetLogger(this.GetType()).Debug(msg) };

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
                throw new ExpectableTestException();
            }

            public int SleepMethodInvocations { get; private set; }
        }
    }
}