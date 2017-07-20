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

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Common.Logging;
using NUnit.Framework;

namespace Proteus.Retry.Test
{
    [TestFixture]
    public class WhenExecutingHappyPathWithRetry
    {
        [Test]
        [Ignore("compiler optimizations get in the way of a meaningful comparison of execution times :(")]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public void PerformanceOverheadIsAcceptable()
        {
            const int ITERATIONS = 10000;

            var instance = new PerformanceTestSpy();

            var nakedInvocationStopwatch = new Stopwatch();

            nakedInvocationStopwatch.Start();

            for (int i = 0; i < ITERATIONS; i++)
            {
                instance.SomeMethodThatSleeps(1);
            }

            nakedInvocationStopwatch.Stop();

            var retry = new Retry { Logger = msg => LogManager.GetLogger(this.GetType()).Debug(msg) };

            var retryInvocationStopwatch = new Stopwatch();

            retryInvocationStopwatch.Start();

            for (int i = 0; i < ITERATIONS; i++)
            {
                retry.Invoke(() => instance.SomeMethodThatSleeps(1));
            }


            retryInvocationStopwatch.Stop();

            Debug.WriteLine("Naked Invocation (ms): {0}", nakedInvocationStopwatch.ElapsedMilliseconds);
            Debug.WriteLine("Retry Invocation (ms): {0}", retryInvocationStopwatch.ElapsedMilliseconds);

            var percentageDifferentialElapsed = PercentageDifferentialElapsed(nakedInvocationStopwatch, retryInvocationStopwatch);
            Debug.WriteLine("Percentage Differential: {0}", percentageDifferentialElapsed);

            Assert.That(percentageDifferentialElapsed, Is.LessThan(0.5), "Performance differential must be less than 1/2 of 1 percent!");
        }

        private double PercentageDifferentialElapsed(Stopwatch expectedLesserDurationStopwatch, Stopwatch expectedGreaterDurationStopwatch)
        {
            var greater = expectedGreaterDurationStopwatch.ElapsedMilliseconds;
            var lesser = expectedLesserDurationStopwatch.ElapsedMilliseconds;

            Assert.That(lesser, Is.LessThanOrEqualTo(greater), string.Format("Unable to continue: Naked stopwatch ({0} ms) not lesser than Retry stopwatch ({1} ms).", lesser, greater));
            return (double)(greater - lesser) / lesser * 100;
        }


        private class PerformanceTestSpy
        {
            [MethodImpl(MethodImplOptions.NoOptimization)]
            public void SomeMethodThatSleeps(int value)
            {
                Thread.Sleep(value);
            }
        }
    }
}