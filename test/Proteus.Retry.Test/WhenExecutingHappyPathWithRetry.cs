using System.Diagnostics;
using System.IO;
using System.Management.Instrumentation;
using System.Runtime.CompilerServices;
using System.Threading;
using NUnit.Framework;

namespace Proteus.Retry.Test
{
    [TestFixture]
    public class WhenExecutingHappyPathWithRetry
    {
        [Test]
        [Ignore("compiler optimizations get in the way of a meaningful comparison of execution times :(")]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public void PeformanceOverheadIsAcceptable()
        {
            const int ITERATIONS = 10000;

            var instance = new PeformanceTestSpy();

            var nakedInvocationStopwatch = new Stopwatch();

            nakedInvocationStopwatch.Start();

            for (int i = 0; i < ITERATIONS; i++)
            {
                instance.SomeMethodThatSleeps(1);
            }

            nakedInvocationStopwatch.Stop();

            var retry = new Retry();

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
            Debug.WriteLine("Percentage Differental: {0}", percentageDifferentialElapsed);

            Assert.That(percentageDifferentialElapsed, Is.LessThan(0.5), "Peformance differential must be less than 1/2 of 1 percent!");
        }

        private double PercentageDifferentialElapsed(Stopwatch expectedLesserDurationStopwatch, Stopwatch expectedGreaterDurationStopwatch)
        {
            var greater = expectedGreaterDurationStopwatch.ElapsedMilliseconds;
            var lesser = expectedLesserDurationStopwatch.ElapsedMilliseconds;

            Assert.That(lesser, Is.LessThanOrEqualTo(greater),string.Format("Unable to continue: Naked stopwatch ({0} ms) not lesser than Retry stopwatch ({1} ms).", lesser, greater));
            return (double)(greater - lesser) / lesser * 100;
        }


        private class PeformanceTestSpy
        {
            [MethodImpl(MethodImplOptions.NoOptimization)]
            public void SomeMethodThatSleeps(int value)
            {
                Thread.Sleep(value);
            }
        }
    }
}