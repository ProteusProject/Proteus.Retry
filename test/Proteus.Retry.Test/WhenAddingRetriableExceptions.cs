using System;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Proteus.Retry.Test
{
    [TestFixture]
    public class WhenAddingRetriableExceptions
    {
        [Test]
        public void ExceptionsAreTracked()
        {
            var policy = new RetryPolicy();

            policy.RegisterRetriableException<ArithmeticException>();
            policy.RegisterRetriableException<ArgumentOutOfRangeException>();

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
        public void AddingSingleExceptionCanReportExceptionTypeAsRetriable()
        {
            var policy = new RetryPolicy();

            policy.RegisterRetriableException<ExpectableTestExecption>();

            Assert.That(policy.IsRetriableException<ExpectableTestExecption>(), Is.True);
        }


        [Test]
        public void AddingMultipleExceptionsAtOnceCanReportExceptionTypesAsRetriable()
        {
            var policy = new RetryPolicy();

            var exceptions = new List<Type> { typeof(ArithmeticException), typeof(ExpectableTestExecption) };

            policy.RegisterRetriableExceptions(exceptions);

            Assert.That(policy.IsRetriableException<ExpectableTestExecption>(), Is.True);
            Assert.That(policy.IsRetriableException<ArithmeticException>(), Is.True);
        }

        [Test]
        public void AddingMultipleExceptionsIgnoresTypesNotDerivedFromException()
        {
            var policy = new RetryPolicy();

            var exceptions = new List<Type> { typeof(ArithmeticException), typeof(ExpectableTestExecption), typeof(Retry) };

            policy.RegisterRetriableExceptions(exceptions);

            Assert.That(policy.IsRetriableException<ExpectableTestExecption>(), Is.True);
            Assert.That(policy.IsRetriableException<ArithmeticException>(), Is.True);
            Assert.That(policy.RetriableExceptions, Has.No.Member(typeof(Retry)));
        }

        [Test]
        public void CanReportExceptionInstanceAsRetriable()
        {
            var policy = new RetryPolicy();

            policy.RegisterRetriableException<ExpectableTestExecption>();

            Assert.That(policy.IsRetriableException(new ExpectableTestExecption()), Is.True);
            Assert.That(policy.IsRetriableException(new Exception()), Is.False);
        }

        [Test]
        public void CanReportDereivedExceptionTypeAsRetriable()
        {
            var policy = new RetryPolicy();

            //register base class ...
            policy.RegisterRetriableException<Exception>();

            //...check for a derived type
            Assert.That(policy.IsRetriableException<ExpectableTestExecption>(), Is.True);
        }

        [Test]
        public void CanReportDerivedExceptionInstanceAsRetriable()
        {
            var policy = new RetryPolicy();

            //register a bass class ...
            policy.RegisterRetriableException<Exception>();

            //...check for instance of derived type
            Assert.That(policy.IsRetriableException(new ExpectableTestExecption()), Is.True);
        }

        [Test]
        public void CanSetDefaultToIgnoreExceptionTypeHierarchy()
        {
            Assume.That(typeof(InheritedTestException).IsSubclassOf(typeof(ExpectableTestExecption)), "Assumed inheritance relationship not present!");

            var policy = new RetryPolicy { MaxRetries = 10, IgnoreInheritanceForRetryExceptions = true };
            policy.RegisterRetriableException<ExpectableTestExecption>();

            var retry = new Retry(policy);
            var instance = new RetriableExceptionsTestSpy();

            //since only the base type ExpectableTestException is registered as retriable, we should get the un-retried derived type execption here...
            Assert.Throws<InheritedTestException>(() => retry.Invoke(() => instance.ThrowException<InheritedTestException>()));
        }

        [Test]
        public void CanOverrideRespectExceptionTypeHierarchySettingForSingleInvocation()
        {
            Assume.That(typeof(InheritedTestException).IsSubclassOf(typeof(ExpectableTestExecption)), "Assumed inheritance relationship not present!");

            var policy = new RetryPolicy { MaxRetries = 10 };
            policy.RegisterRetriableException<ExpectableTestExecption>();

            Assume.That(policy.IgnoreInheritanceForRetryExceptions, Is.False);

            var retry = new Retry(policy);
            var instance = new RetriableExceptionsTestSpy();

            //since only the base type ExpectableTestException is registered as retriable, we should get the un-retried derived type execption here...
            Assert.Throws<InheritedTestException>(() => retry.Invoke(() => instance.ThrowException<InheritedTestException>(), true));
        }

        [Test]
        public void CanOverrideIgnoreExceptionTypeHierarchySettingForSingleInvocation()
        {
            Assume.That(typeof(InheritedTestException).IsSubclassOf(typeof(ExpectableTestExecption)), "Assumed inheritance relationship not present!");

            var policy = new RetryPolicy { MaxRetries = 10, IgnoreInheritanceForRetryExceptions = true };
            policy.RegisterRetriableException<ExpectableTestExecption>();

            var retry = new Retry(policy);
            var instance = new RetriableExceptionsTestSpy();

            //since only the base type ExpectableTestException is registered as retriable, we should get the un-retried derived type execption here...
            Assert.Throws<InheritedTestException>(() => retry.Invoke(() => instance.ThrowException<InheritedTestException>(), false));
        }

        private class InheritedTestException : ExpectableTestExecption
        {

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