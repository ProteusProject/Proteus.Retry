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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using NUnit.Framework;
using Proteus.Retry.Exceptions;

namespace Proteus.Retry.Test
{
    [TestFixture]
    public class WhenInvokingAsyncDelegates
    {
        [Test]
        public async Task CanAwaitSuccessfulInvocation()
        {
            var retry = new Retry { Logger = msg => LogManager.GetLogger(this.GetType()).Debug(msg) };

            var instance = new TestSpy();

            var result = await retry.Invoke(() => instance.AwaitableMethodThatDoesntThrow("invoked!"));

            Assert.That(result.Contains("invoked!"));
        }

        [Test]
        public async Task CanReportMaxRetriesExceededIfNoSuccess()
        {

            const int MAX_RETRIES = 20;

            var policy = new RetryPolicy();
            policy.RegisterRetriableException<ExpectableTestException>();
            policy.MaxRetries = MAX_RETRIES;

            var retry = new Retry(policy) { Logger = msg => LogManager.GetLogger(this.GetType()).Debug(msg) };

            var instance = new TestSpy();

            try
            {
                await retry.Invoke(() => instance.AwaitableMethodThatAlwaysThrowsImmediately());
                Assert.Fail("MaxRetryCountExceededException not thrown.");
            }
            catch (MaxRetryCountExceededException)
            {
                Assert.Pass();
            }

            Assert.That(instance.InvocationsOfAwaitableMethodThatAlwaysThrowsImmediately, Is.EqualTo(MAX_RETRIES + 1));
        }

        [Test]
        public async Task CanPopulateInnerExceptionHistoryWithInnerExceptions()
        {

            const int MAX_RETRIES = 20;

            var policy = new RetryPolicy();
            policy.RegisterRetriableException<ExpectableTestException>();
            policy.MaxRetries = MAX_RETRIES;

            var retry = new Retry(policy) { Logger = msg => LogManager.GetLogger(this.GetType()).Debug(msg) };

            var instance = new TestSpy();

            try
            {
                await retry.Invoke(() => instance.AwaitableMethodThatAlwaysThrowsImmediately());
                Assert.Fail("MaxRetryCountExceededException not thrown.");
            }
            catch (MaxRetryCountExceededException exception)
            {
                Assert.That(exception.InnerExceptionHistory.Any(ex => ex.GetType() == typeof(ExpectableTestException)));
            }
        }

        [Test]
        public async Task CanPopulateInnerExceptionHistoryWithInnerExceptionsWhenNestedAggregateExceptions()
        {

            const int MAX_RETRIES = 20;

            var policy = new RetryPolicy();
            policy.RegisterRetriableException<ExpectableTestException>();
            policy.MaxRetries = MAX_RETRIES;

            var retry = new Retry(policy) { Logger = msg => LogManager.GetLogger(this.GetType()).Debug(msg) };

            var instance = new TestSpy();

            try
            {
                await retry.Invoke(() => instance.AwaitableMethodThatAlwaysThrowsAndCallsNestedAwaitableMethodThatAlwaysThrows());
                Assert.Fail("MaxRetryCountExceededException not thrown.");
            }
            catch (MaxRetryCountExceededException exception)
            {
                Assert.That(exception.InnerExceptionHistory.Any(ex => ex.GetType() == typeof(ExpectableTestException)));
            }
        }


        [Test]
        public async Task CanReportMaxRetryDurationExceededIfNoSuccess()
        {
            var policy = new RetryPolicy { MaxRetries = 10, MaxRetryDuration = TimeSpan.FromSeconds(5) };
            policy.RegisterRetriableException<ExpectableTestException>();

            var instance = new TestSpy();

            var retry = new Retry(policy) { Logger = msg => LogManager.GetLogger(this.GetType()).Debug(msg) };

            try
            {
                await retry.Invoke(() => instance.AwaitableMethodThatAlwaysThrowsAfterSleepingFor(2000));
            }
            catch (MaxRetryDurationExpiredException)
            {
                Assert.Pass();
            }
            catch (Exception exception)
            {
                Assert.Fail("Got: {0}: {1}", exception.GetType(), exception.Message);
            }
        }


        public class TestSpy
        {
            public string NonAwaitableMethodThatDoesntThrow(string message)
            {
                Thread.Sleep(5000);
                return string.Format("The method was {0}", message);
            }


            public async Task<int> SomeMethodThatAwaits()
            {
                return await Task.Run(() => DoWork());
            }

            private int DoWork()
            {
                Thread.Sleep(1000);
                throw new ExpectableTestException();
            }

            public Task<string> AwaitableMethodThatDoesntThrow(string message)
            {
                return Task.Run(() => NonAwaitableMethodThatDoesntThrow(message));
            }

            public async Task AwaitableMethodThatThrowsUntilInvocationCountIs(int throwInvocationCount)
            {
                await Task.Run(() =>
                {
                    if (InvocationsOfAwaitableMethodThatThrowsUntil < throwInvocationCount)
                    {
                        InvocationsOfAwaitableMethodThatThrowsUntil++;
                        throw new ExpectableTestException();
                    }
                });
            }

            public Task AwaitableMethodThatAlwaysThrowsImmediately()
            {
                InvocationsOfAwaitableMethodThatAlwaysThrowsImmediately++;
                throw new ExpectableTestException();
            }

            public Task AwaitableMethodThatAlwaysThrowsAfterSleepingFor(int milliseconds)
            {
                InvocationsOfAwaitableMethodThatAlwaysThrowsAfterSleeping++;
                Thread.Sleep(milliseconds);
                throw new ExpectableTestException();
            }

            public int InvocationsOfAwaitableMethodThatAlwaysThrowsAfterSleeping { get; private set; }
            public int InvocationsOfAwaitableMethodThatAlwaysThrowsImmediately { get; private set; }
            public int InvocationsOfAwaitableMethodThatThrowsUntil { get; private set; }

            public async Task AwaitableMethodThatAlwaysThrowsAndCallsNestedAwaitableMethodThatAlwaysThrows()
            {
                await AwaitableMethodThatAlwaysThrowsImmediately();
                throw new ExpectableTestException();
            }
        }
    }
}