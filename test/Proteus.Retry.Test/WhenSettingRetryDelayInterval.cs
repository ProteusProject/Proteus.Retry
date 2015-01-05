using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Metadata;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using NUnit.Framework;
using Proteus.Retry.Exceptions;

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
            policy.RetryDelayIntervalProvider = () => TimeSpan.FromSeconds(1);

            Assert.That(policy.RetryDelayInterval, Is.EqualTo(TimeSpan.FromSeconds(1)));
        }

        [Test]
        public void RetryDelayIntervalCalculationStrategyOverridesExplicitlySetDelayInterval()
        {
            var policy = new RetryPolicy();
            policy.RetryDelayInterval = TimeSpan.FromHours(1);
            Assume.That(policy.RetryDelayInterval, Is.EqualTo(TimeSpan.FromHours(1)), "Explicitly-set RetryDelayInterval not found!");

            policy.RetryDelayIntervalProvider = () => TimeSpan.FromSeconds(1);

            Assert.That(policy.RetryDelayInterval, Is.EqualTo(TimeSpan.FromSeconds(1)));
        }

        [Test]
        public void CanRespectRetryDelayIntervalCalculatorMethod()
        {
            var intervalCalculator = new TestRetryDelayIntervalProvider();

            var policy = new RetryPolicy();
            policy.RetryDelayIntervalProvider = () => intervalCalculator.DoublePriorInterval();
            Assume.That(intervalCalculator.Interval, Is.EqualTo(TimeSpan.FromMilliseconds(1)));

            var instance = new RetryDelayIntervalTestSpy();

            var retry = new Retry
            {
                MaxRetries = 10,
                RetryDelayIntervalProvider = intervalCalculator.DoublePriorInterval
            };
            retry.RegisterRetriableException<ExpectableTestExecption>();

            Assert.Throws<MaxRetryCountExceededException>(() => retry.Invoke(() => instance.MethodThatAlwaysThrows()),
                "Did not get to end of retries count!");

            var deltas = new List<double>();


            //the first intervasl measure is always invalid so have to start Asserts with the second pair
            // of measuresments (which means index = 2)
            for (var i = 2; i < instance.Intervals.Count; i++)
            {
                var currentInterval = instance.Intervals[i];
                var priorInterval = instance.Intervals[i - 1];
                deltas.Add((currentInterval - priorInterval) / (double)priorInterval);

            }

            //permit up to 20% of the timings to be out-of-spec; necessary to accommodate variations in timing
            // during test-runs, else test results are too indeterminate to be useful
            var results = deltas.Select(delta => AreEqualWithinTolerance(delta, 1.0, 0.20)).ToList();
            var falseResultsCount = results.Count(r => r == false);
            
            Assert.That((double)falseResultsCount / results.Count, Is.LessThanOrEqualTo(0.20));

        }


        private bool AreEqualWithinTolerance(double first, double second, double tolerance)
        {
            return Math.Abs(first - second) <= tolerance;
        }

        [Test]
        public void TestRetryDelayIntervalProviderBehavesAsNeeded()
        {
            var intervalCalculator = new TestRetryDelayIntervalProvider();
            Assume.That(intervalCalculator.Interval, Is.EqualTo(TimeSpan.FromMilliseconds(1)));

            Assert.That(intervalCalculator.DoublePriorInterval(), Is.EqualTo(TimeSpan.FromMilliseconds(2)));
            Assert.That(intervalCalculator.DoublePriorInterval(), Is.EqualTo(TimeSpan.FromMilliseconds(4)));

            Assert.That(intervalCalculator.Interval, Is.EqualTo(TimeSpan.FromMilliseconds(4)));

        }

        private class TestRetryDelayIntervalProvider
        {
            private TimeSpan _interval = TimeSpan.FromMilliseconds(1);

            public TimeSpan Interval
            {
                get { return _interval; }
            }

            public TimeSpan DoublePriorInterval()
            {
                //double it and store that value in the field so we can double it again next time...
                // NOTE: must perform math against TimeSpan.Ticks b/c its the only 'absolute' measure
                //       reportable by TimeSpans
                _interval = TimeSpan.FromTicks(_interval.Ticks * 2);
                return _interval;
            }
        }

        private class RetryDelayIntervalTestSpy
        {
            public readonly IList<long> Intervals = new List<long>();
            private long _priorInvocationMs;

            public void MethodThatAlwaysThrows()
            {
                long currentInvocationMs = DateTime.Now.Ticks;

                //if we're not in the first call to the method...
                if (_priorInvocationMs > 0)
                {
                    //...add the interval to the collection for later assert
                    Intervals.Add(currentInvocationMs - _priorInvocationMs);
                }

                _priorInvocationMs = currentInvocationMs;
                InvocationsOfMethodThatAlwaysThrows++;
                throw new ExpectableTestExecption();
            }

            public int InvocationsOfMethodThatAlwaysThrows { get; private set; }
        }

    }
}