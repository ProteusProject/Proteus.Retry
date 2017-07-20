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
using Common.Logging;
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
            retry.Logger = msg => LogManager.GetLogger(this.GetType()).Debug(msg);
            var instance = new RetriableExceptionsTestSpy();

            Assert.Throws<ExpectableTestException>(() => retry.Invoke(() => instance.ThrowException<ExpectableTestException>()));

        }

        [Test]
        public void AddingSingleExceptionCanReportExceptionTypeAsRetriable()
        {
            var policy = new RetryPolicy();

            policy.RegisterRetriableException<ExpectableTestException>();

            Assert.That(policy.IsRetriableException<ExpectableTestException>(), Is.True);
        }


        [Test]
        public void AddingMultipleExceptionsAtOnceCanReportExceptionTypesAsRetriable()
        {
            var policy = new RetryPolicy();

            var exceptions = new List<Type> { typeof(ArithmeticException), typeof(ExpectableTestException) };

            policy.RegisterRetriableExceptions(exceptions);

            Assert.That(policy.IsRetriableException<ExpectableTestException>(), Is.True);
            Assert.That(policy.IsRetriableException<ArithmeticException>(), Is.True);
        }

        [Test]
        public void CanPreventAddingMultipleExceptionsContainingTypesNotDerivedFromException()
        {
            var policy = new RetryPolicy();
            var exceptions = new List<Type> { typeof(ArithmeticException), typeof(ExpectableTestException), typeof(Retry) };

            Assert.Throws<ArgumentException>(() => policy.RegisterRetriableExceptions(exceptions));
        }

        [Test]
        public void CanReportExceptionInstanceAsRetriable()
        {
            var policy = new RetryPolicy();

            policy.RegisterRetriableException<ExpectableTestException>();

            Assert.That(policy.IsRetriableException(new ExpectableTestException()), Is.True);
            Assert.That(policy.IsRetriableException(new Exception()), Is.False);
        }

        [Test]
        public void CanReportDereivedExceptionTypeAsRetriable()
        {
            var policy = new RetryPolicy();

            //register base class ...
            policy.RegisterRetriableException<Exception>();

            //...check for a derived type
            Assert.That(policy.IsRetriableException<ExpectableTestException>(), Is.True);
        }

        [Test]
        public void CanReportDerivedExceptionInstanceAsRetriable()
        {
            var policy = new RetryPolicy();

            //register a bass class ...
            policy.RegisterRetriableException<Exception>();

            //...check for instance of derived type
            Assert.That(policy.IsRetriableException(new ExpectableTestException()), Is.True);
        }

        [Test]
        public void CanSetDefaultToIgnoreExceptionTypeHierarchy()
        {
            Assume.That(typeof(DerivedTestException).IsSubclassOf(typeof(ExpectableTestException)), "Assumed inheritance relationship not present!");

            var policy = new RetryPolicy { MaxRetries = 10, IgnoreInheritanceForRetryExceptions = true };
            policy.RegisterRetriableException<ExpectableTestException>();

            var retry = new Retry(policy);
            retry.Logger = msg => LogManager.GetLogger(this.GetType()).Debug(msg);
            var instance = new RetriableExceptionsTestSpy();

            //since only the base type ExpectableTestException is registered as retriable, we should get the un-retried derived type exception here...
            Assert.Throws<DerivedTestException>(() => retry.Invoke(() => instance.ThrowException<DerivedTestException>()));
        }

        private class DerivedTestException : ExpectableTestException
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