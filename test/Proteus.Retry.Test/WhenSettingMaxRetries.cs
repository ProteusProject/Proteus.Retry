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
            policy.RegisterRetriableException<ExpectableTestException>();

            var testObject = new RetryPolicyMaxRetriesTestSpy();

            var retry = new Retry(policy);
            retry.Logger = msg => LogManager.GetLogger(this.GetType()).Debug(msg);

            Assert.Throws<MaxRetryCountExceededException>(() => retry.Invoke(() => testObject.DoWorkThatAlwaysThrows()));
            Assert.That(testObject.InvocationsOfDoWorkThatAlwaysThrows, Is.EqualTo(MAX_RETRIES + 1));
        }

        [Test]
        public void ZeroMaxRetriesStillInvokesDelegateOnce()
        {
            var policy = new RetryPolicy { MaxRetries = 0 };
            policy.RegisterRetriableException<ExpectableTestException>();

            var testObject = new RetryPolicyMaxRetriesTestSpy();

            var retry = new Retry(policy);
            retry.Logger = msg => LogManager.GetLogger(this.GetType()).Debug(msg);

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
            policy.RegisterRetriableException<ExpectableTestException>();

            var testObject = new RetryPolicyMaxRetriesTestSpy();

            var retry = new Retry(policy);
            retry.Logger = msg => LogManager.GetLogger(this.GetType()).Debug(msg);

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
            policy.RegisterRetriableException<ExpectableTestException>();

            var testObject = new RetryPolicyMaxRetriesTestSpy();

            var retry = new Retry(policy);
            retry.Logger = msg => LogManager.GetLogger(this.GetType()).Debug(msg);

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
                    Assert.That(pastException, Is.InstanceOf<ExpectableTestException>());
                }
            }
        }

        private class RetryPolicyMaxRetriesTestSpy
        {
            public void DoWorkThatAlwaysThrows()
            {
                InvocationsOfDoWorkThatAlwaysThrows++;
                throw new ExpectableTestException();
            }

            public void DoWorkThatThrowsUntilInvocationCountIs(int throwInvocationCount)
            {
                if (InvocationsOfDoWorkThatThrowsUntil < throwInvocationCount)
                {
                    InvocationsOfDoWorkThatThrowsUntil++;
                    throw new ExpectableTestException();
                }
            }

            public int InvocationsOfDoWorkThatAlwaysThrows { get; private set; }
            public int InvocationsOfDoWorkThatThrowsUntil { get; private set; }
        }
    }
}
