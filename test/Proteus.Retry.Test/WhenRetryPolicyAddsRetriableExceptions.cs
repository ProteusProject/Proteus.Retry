using System;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Proteus.Retry.Test
{
    [TestFixture]
    public class WhenRetryPolicyAddsRetriableExceptions
    {
        [Test]
        public void ExceptionsAreTracked()
        {
            var policy = new RetryPolicy();

            policy.RetryOnException<ArithmeticException>();
            policy.RetryOnException<ArgumentOutOfRangeException>();

            Assert.That(policy.RetriableExceptions, Has.Member(typeof(ArithmeticException)));
            Assert.That(policy.RetriableExceptions, Has.Member(typeof(ArgumentOutOfRangeException)));
        }

        [Test]
        public void RetryThrowsIfNoRetriableExceptions()
        {
            var policy = new RetryPolicy();
            Assume.That(policy.RetriableExceptions, Is.Empty);


            var retry = new Retry(policy);
            var instance = new RetriableExceptionsTestSpy();

            Assert.Throws<ExpectableTestExecption>(() => retry.Invoke(() => instance.ThrowException<ExpectableTestExecption>()));

        }

        [Test]
        public void CanReportExceptionTypeAsRetriable()
        {
            var policy = new RetryPolicy();

            policy.RetryOnException<ExpectableTestExecption>();

            Assert.That(policy.IsRetryException<ExpectableTestExecption>(), Is.True);
            Assert.That(policy.IsRetryException<Exception>(), Is.False);
        }

        [Test]
        public void CanReportExceptionInstanceAsRetriable()
        {
            var policy = new RetryPolicy();

            policy.RetryOnException<ExpectableTestExecption>();

            Assert.That(policy.IsRetryException(new ExpectableTestExecption()), Is.True);
            Assert.That(policy.IsRetryException(new Exception()), Is.False);
        }

        [Test]
        public void CanReportDereivedExceptionTypeAsRetriable()
        {
            var policy = new RetryPolicy();
            
            //register base class ...
            policy.RetryOnException<Exception>();

            //...check for a derived type
            Assert.That(policy.IsRetryException<ExpectableTestExecption>(), Is.True);
        }

        [Test]
        public void CanReportDerivedExceptionInstanceAsRetriable()
        {
            var policy = new RetryPolicy();

            //register a bass class ...
            policy.RetryOnException<Exception>();
            
            //...check for instance of derived type
            Assert.That(policy.IsRetryException(new ExpectableTestExecption()), Is.True);
        }

        private class RetriableExceptionsTestSpy
        {
            public void ThrowException<TException>() where TException : Exception, new()
            {
                throw new TException();
            }
        }
    }


}