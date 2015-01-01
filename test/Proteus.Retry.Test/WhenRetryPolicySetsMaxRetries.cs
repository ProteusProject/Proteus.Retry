using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Proteus.Retry.Test
{
    [TestFixture]
    public class WhenRetryPolicySetsMaxRetries
    {
        [Test]
        public void MaxRetriesAreRespected()
        {
            const int MAX_RETRIES = 3;

            var policy = new RetryPolicy { MaxRetries = MAX_RETRIES };

            var testObject = new RetryPolicyMaxRetriesTestSpy();

            var retry = new Retry(policy);

            retry.Invoke(() => testObject.DoWorkThatAlwaysThrows());

            Assert.That(testObject.DoWorkThatAlwaysThrowsInvocationCounter, Is.EqualTo(MAX_RETRIES + 1));
        }

        [Test]
        public void MaxRetriesOfZeroInvokesDelegateOnce()
        {
            var policy = new RetryPolicy { MaxRetries = 0 };

            var testObject = new RetryPolicyMaxRetriesTestSpy();

            var retry = new Retry(policy);

            retry.Invoke(() => testObject.DoWorkThatAlwaysThrows());

            Assert.That(testObject.DoWorkThatAlwaysThrowsInvocationCounter, Is.EqualTo(1));
        }

        [Test]
        public void WillNotRetryAgainAfterSuccessfulInvocation()
        {
            var policy = new RetryPolicy { MaxRetries = 20 };

            var testObject = new RetryPolicyMaxRetriesTestSpy();

            var retry = new Retry(policy);

            retry.Invoke(() => testObject.DoWorkThatThrowsUntilInvocationCountIs(3));

            Assert.That(testObject.DoWorkThatThrowsUntilInvocationCounter, Is.EqualTo(3));
        }
        
        class RetryPolicyMaxRetriesTestSpy
        {
            public void DoWorkThatAlwaysThrows()
            {
                DoWorkThatAlwaysThrowsInvocationCounter++;
                throw new Exception();
            }

            public void DoWorkThatThrowsUntilInvocationCountIs(int throwInvocationCount)
            {
                if (DoWorkThatThrowsUntilInvocationCounter < throwInvocationCount)
                {
                    DoWorkThatThrowsUntilInvocationCounter++;
                    throw new Exception();
                }
            }

            public int DoWorkThatAlwaysThrowsInvocationCounter { get; private set; }
            public int DoWorkThatThrowsUntilInvocationCounter { get; private set; }
        }
    }

    
}
