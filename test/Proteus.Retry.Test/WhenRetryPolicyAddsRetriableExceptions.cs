using System;
using NUnit.Framework;

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
    }

    
}