using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Proteus.Retry.Test
{
    [TestFixture]
    public class WhenSettingDefaultLogger
    {
        private DefaultLoggerTracingSpy _defaultLoggerTracingSpy;

        [SetUp]
        public void TestSetUp()
        {
            _defaultLoggerTracingSpy = new DefaultLoggerTracingSpy();
            Retry.DefaultLogger = msg => _defaultLoggerTracingSpy.LogMessage(msg);
        }

        [Test]
        public void DefaultLoggerIsUsedForNewRetryInstances()
        {
            Retry.Using(new RetryPolicy()).Invoke(() => 2 + 2);
            Assert.That(_defaultLoggerTracingSpy.Messages, Is.Not.Empty);
        }

        [Test]
        public void CanOverrideDefaultLoggerWithExplicitLoggerOnRetryInstance()
        {
            var explicitTracingSpy = new ExplicitInstanceLoggerTracingSpy();

            var retry = new Retry { Logger = msg => explicitTracingSpy.LogMessage(msg) };
            retry.Invoke(() => 2 + 2);

            Assert.That(_defaultLoggerTracingSpy.Messages, Is.Empty, "DefaultLogger improperly used.");
            Assert.That(explicitTracingSpy.Messages, Is.Not.Empty, "Explicit instance Logger failed to override DefaultLogger.");
        }


        private class LoggerTracingSpy
        {
            public IList<string> Messages { get; } = new List<string>();

            public void LogMessage(string message)
            {
                Messages.Add(message);
            }
        }

        private class DefaultLoggerTracingSpy : LoggerTracingSpy
        {

        }

        private class ExplicitInstanceLoggerTracingSpy : LoggerTracingSpy
        {

        }
    }
}