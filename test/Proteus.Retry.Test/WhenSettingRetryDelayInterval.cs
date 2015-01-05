using System;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using NUnit.Framework;

namespace Proteus.Retry.Test
{
    [TestFixture]
    public class WhenSettingRetryDelayInterval
    {
        [Test]
        public void DefaultIntervalIsZero()
        {
            var policy = new RetryPolicy();
            Assert.That(policy.RetryDelayInterval, Is.EqualTo(TimeSpan.FromSeconds(0)));
        }

        [Test]
        public void CanSetAndRetrieveExplicitInterval()
        {
            var policy = new RetryPolicy();
            policy.RetryDelayInterval = TimeSpan.FromHours(1);

            Assert.That(policy.RetryDelayInterval, Is.EqualTo(TimeSpan.FromHours(1)));
        }

        [Test]
        public void CanUseRetryDelayIntervalCalculator()
        {
            var policy = new RetryPolicy();
            policy.RetryDelayIntervalCalculator = () => TimeSpan.FromSeconds(1);

            Assert.That(policy.RetryDelayInterval, Is.EqualTo(TimeSpan.FromSeconds(1)));
        }

        [Test]
        public void RetryDelayIntervalCalculationStrategyOverridesExplicitlySetDelayInterval()
        {
            var policy = new RetryPolicy();
            policy.RetryDelayInterval = TimeSpan.FromHours(1);
            Assume.That(policy.RetryDelayInterval, Is.EqualTo(TimeSpan.FromHours(1)),"Explicitly-set RetryDelayInterval not found!");

            policy.RetryDelayIntervalCalculator = () => TimeSpan.FromSeconds(1);

            Assert.That(policy.RetryDelayInterval, Is.EqualTo(TimeSpan.FromSeconds(1)));
        }

        
    }
}