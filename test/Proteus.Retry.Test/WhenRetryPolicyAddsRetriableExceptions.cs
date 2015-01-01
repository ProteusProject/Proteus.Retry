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

            policy.AddRetriableException<Exception>();
            policy.AddRetriableException<ArgumentOutOfRangeException>();

            Assert.That(policy.RetriableExceptions, Has.Member(typeof(Exception)));
            Assert.That(policy.RetriableExceptions, Has.Member(typeof(ArgumentOutOfRangeException)));
        }

        [Test]
        public void RetryThrowsIfNoRetriableExceptions()
        {
            var policy = new RetryPolicy();
            Assume.That(policy.RetriableExceptions, Is.Empty);


            var retry = new Retry(policy);
            var instance = new RetriableExceptionsTestSpy();

            Assert.Throws<ArithmeticException>(() => retry.Invoke(() => instance.ThrowException<ArithmeticException>()));

        }

        [Test]
        public void CanReportExceptionTypeAsRetriable()
        {
            var policy = new RetryPolicy();

            policy.AddRetriableException<ArithmeticException>();

            Assert.That(policy.IsRetriableException<ArithmeticException>(), Is.True);
            Assert.That(policy.IsRetriableException<Exception>(), Is.False);
        }

        [Test]
        public void CanReportExceptionInstanceAsRetriable()
        {
            var policy = new RetryPolicy();

            policy.AddRetriableException<ArithmeticException>();

            Assert.That(policy.IsRetriableException(new ArithmeticException()), Is.True);
            Assert.That(policy.IsRetriableException(new Exception()), Is.False);
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