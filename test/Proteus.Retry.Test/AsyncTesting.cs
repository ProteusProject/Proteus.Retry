using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Proteus.Retry.Exceptions;

namespace Proteus.Retry.Test
{
    [TestFixture]
    public class AsyncTesting
    {
        [Test]
        public async void HappyPath1()
        {
            var retry = new Retry();

            var instance = new TestSpy();

            var result = await retry.Invoke(() => instance.AwaitableMethodThatDoesntThrow("invoked!"));

            Assert.That(result.Contains("invoked!"));
        }

        [Test]
        public async void UnhappyPath()
        {
            var policy = new RetryPolicy();
            policy.RegisterRetriableException<ExpectableTestExecption>();
            policy.MaxRetries = 2;

            var retry = new Retry(policy);

            var instance = new TestSpy();


            try
            {
                await retry.Invoke(() => instance.AsyncMethodThatThrowsUntilInvocationCountIs(3));
                Assert.Fail("MaxRetryCountExceededException not thrown.");
            }
            catch (MaxRetryCountExceededException)
            {
                //swallow so that test will pass if we end up here!
            }
            
            Assert.That(instance.InvocationsOfAsyncMethodThatThrowsUntil, Is.EqualTo(3));
        }


        [Test]
        public async Task MyMethod()
        {
            var policy = new RetryPolicy { MaxRetries = 10, MaxRetryDuration = TimeSpan.FromSeconds(5) };
            policy.RegisterRetriableException<ExpectableTestExecption>();

            var instance = new TestSpy();

            var retry = new Retry(policy);

            try
            {
                var x = retry.Invoke(() => instance.SomeMethodThatAwaits());
                await x;

            }
            catch (ExpectableTestExecption execption)
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

            public async Task AsyncMethodThatThrowsUntilInvocationCountIs(int throwInvocationCount)
            {
                //await Task.Run(() =>
                //{
                if (InvocationsOfAsyncMethodThatThrowsUntil < throwInvocationCount)
                {
                    InvocationsOfAsyncMethodThatThrowsUntil++;
                    throw new ExpectableTestExecption();
                }
                //});
            }

            public int InvocationsOfAsyncMethodThatThrowsUntil { get; set; }
        }
    }
}