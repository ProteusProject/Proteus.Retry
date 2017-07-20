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
using System.Linq;
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
            Assert.That(policy.NextRetryDelayInterval(), Is.EqualTo(TimeSpan.FromSeconds(0)));
        }

        [Test]
        public void CanSetAndRetrieveExplicitInterval()
        {
            var policy = new RetryPolicy { RetryDelayInterval = TimeSpan.FromHours(1) };

            Assert.That(policy.NextRetryDelayInterval(), Is.EqualTo(TimeSpan.FromHours(1)));
        }

        [Test]
        public void CanUseRetryDelayIntervalProvider()
        {
            var policy = new RetryPolicy { RetryDelayIntervalProvider = () => TimeSpan.FromSeconds(1) };

            Assert.That(policy.NextRetryDelayInterval(), Is.EqualTo(TimeSpan.FromSeconds(1)));
        }

        [Test]
        public void RetryDelayIntervalProviderStrategyOverridesExplicitlySetDelayInterval()
        {
            var policy = new RetryPolicy { RetryDelayInterval = TimeSpan.FromHours(1) };
            Assume.That(policy.RetryDelayInterval, Is.EqualTo(TimeSpan.FromHours(1)), "Explicitly-set RetryDelayInterval not found!");

            policy.RetryDelayIntervalProvider = () => TimeSpan.FromSeconds(1);

            Assert.That(policy.NextRetryDelayInterval(), Is.EqualTo(TimeSpan.FromSeconds(1)));
        }

        [Test]
        public void CanRespectRetryDelayIntervalProviderMethod()
        {
            var intervalProvider = new TestRetryDelayIntervalProvider();

            var policy = new RetryPolicy { RetryDelayIntervalProvider = () => intervalProvider.DoublePriorInterval() };
            Assume.That(intervalProvider.Interval, Is.EqualTo(TimeSpan.FromMilliseconds(10)), "initial interval not set to expected TimeSpan");

            policy.MaxRetries = 5;
            policy.RegisterRetriableException<ExpectableTestException>();

            var instance = new RetryDelayIntervalTestSpy();

            var retry = new Retry(policy);

            Assert.Throws<MaxRetryCountExceededException>(() => retry.Invoke(() => instance.MethodThatAlwaysThrows()),
                "Did not get to end of retries count!");

            var deltas = new List<double>();


            //the first interval measure is always invalid so have to start Asserts with the second pair
            // of measurements (which means index = 2)
            for (var i = 2; i < instance.Intervals.Count; i++)
            {
                var currentInterval = instance.Intervals[i];
                var priorInterval = instance.Intervals[i - 1];
                deltas.Add((currentInterval - priorInterval) / (double)priorInterval);

            }

            //permit up to 33% of the timings to be out-of-spec; necessary to accommodate variations in timing
            // during test-runs, else test results are too indeterminate to be useful :(
            var results = deltas.Select(delta => AreEqualWithinTolerance(delta, 1.0, 0.20)).ToList();
            var falseResultsCount = results.Count(r => r == false);

            Assert.That((double)falseResultsCount / results.Count, Is.LessThanOrEqualTo(0.34));

        }

        private bool AreEqualWithinTolerance(double first, double second, double tolerance)
        {
            return Math.Abs(first - second) <= tolerance;
        }

        [Test]
        public void TestRetryDelayIntervalProviderBehavesAsNeeded()
        {
            var intervalProvider = new TestRetryDelayIntervalProvider();
            Assume.That(intervalProvider.Interval, Is.EqualTo(TimeSpan.FromMilliseconds(10)));

            Assert.That(intervalProvider.DoublePriorInterval(), Is.EqualTo(TimeSpan.FromMilliseconds(20)));
            Assert.That(intervalProvider.DoublePriorInterval(), Is.EqualTo(TimeSpan.FromMilliseconds(40)));

            Assert.That(intervalProvider.Interval, Is.EqualTo(TimeSpan.FromMilliseconds(40)));

        }

        private class TestRetryDelayIntervalProvider
        {
            private TimeSpan _interval = TimeSpan.FromMilliseconds(10);

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
            private long _priorInvocationTicks;

            public void MethodThatAlwaysThrows()
            {
                long currentInvocationTicks = DateTime.Now.Ticks;

                //if we're not in the first call to the method...
                if (_priorInvocationTicks > 0)
                {
                    //...add the interval to the collection for later assert
                    Intervals.Add(currentInvocationTicks - _priorInvocationTicks);
                }

                _priorInvocationTicks = currentInvocationTicks;
                InvocationsOfMethodThatAlwaysThrows++;
                throw new ExpectableTestException();
            }

            public int InvocationsOfMethodThatAlwaysThrows { get; private set; }
        }

    }
}