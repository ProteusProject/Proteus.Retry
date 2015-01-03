using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
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
            var retry = new Retry();

            var instance = new TestSpy();

            var result = await retry.Invoke(() => instance.AwaitableMethodThatDoesntThrow("invoked!"));

            Assert.That(result.Contains("invoked!"));
        }

        [Test]
        public async Task CanReportMaxRetriesExceededIfNoSuccess()
        {

            const int MAX_RETRIES = 20;

            var policy = new RetryPolicy();
            policy.RegisterRetriableException<ExpectableTestExecption>();
            policy.MaxRetries = MAX_RETRIES;

            var retry = new Retry(policy);

            var instance = new TestSpy();

            try
            {
                await retry.Invoke(() => instance.AwaitableMethodThatAlwaysThrows());
                Assert.Fail("MaxRetryCountExceededException not thrown.");
            }
            catch (MaxRetryCountExceededException)
            {
                Assert.Pass();
            }

            Assert.That(instance.InvocationsOfAwaitableMethodThatAlwaysThrows, Is.EqualTo(MAX_RETRIES + 1));
        }


        [Test]
        public async Task CanReportMaxRetryDurationExceededIfNoSuccess()
        {
            var policy = new RetryPolicy { MaxRetries = 10, MaxRetryDuration = TimeSpan.FromSeconds(5) };
            policy.RegisterRetriableException<ExpectableTestExecption>();

            var instance = new TestSpy();

            var retry = new Retry(policy);

            try
            {
                await retry.Invoke(() => instance.AwaitableMethodThatAlwaysThrowsAndSleepsFor(2000));
            }
            catch (MaxRetryDurationExpiredException execption)
            {
                Assert.Pass();
            }
            catch (Exception exception)
            {
                Assert.Fail(string.Format("Got: {0}: {1}", exception.GetType(), exception.Message));
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
                throw new ExpectableTestExecption();
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
                        throw new ExpectableTestExecption();
                    }
                });
            }

            public Task AwaitableMethodThatAlwaysThrows()
            {
                InvocationsOfAwaitableMethodThatAlwaysThrows++;
                throw new ExpectableTestExecption();
            }
            
            public Task AwaitableMethodThatAlwaysThrowsAndSleepsFor(int milliseconds)
            {
                InvocationsOfAwaitableMethodThatAlwaysThrowsAndSleeps++;
                Thread.Sleep(milliseconds);
                throw new ExpectableTestExecption();
            }

            public int InvocationsOfAwaitableMethodThatAlwaysThrowsAndSleeps { get; private set; }
            public int InvocationsOfAwaitableMethodThatAlwaysThrows { get; private set; }
            public int InvocationsOfAwaitableMethodThatThrowsUntil { get; private set; }
        }
    }
}