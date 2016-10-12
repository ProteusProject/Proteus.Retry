using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using NUnit.Framework;

namespace Proteus.Retry.Test
{
    [TestFixture]
    public class GeneralUsageSample
    {
        [Test]
        public void GeneralScenarioUsingInstanceRetrySyntax()
        {
            var instance = new TestObject();

            var policy = new RetryPolicy { MaxRetries = 20 };
            policy.RegisterRetriableException<ExpectableTestException>();

            var retry = new Retry(policy);

            retry.Logger = LogManager.GetLogger(this.GetType());

            var result = retry.Invoke(() => instance.IntReturningMethod(1, "func invoked"));

            Assert.That(result, Is.EqualTo(1));
            Assert.That(instance.IntResult, Is.EqualTo(1));
            Assert.That(instance.StringResult, Is.EqualTo("func invoked"));

            retry.Invoke(() => instance.VoidReturningMethodThatThrowsOnFirstInvocation(2, "action invoked"));

            Assert.That(instance.IntResult, Is.EqualTo(2));
            Assert.That(instance.StringResult, Is.EqualTo("action invoked"));

            Assert.That(instance.IntReturnInvokeCount, Is.EqualTo(1));
            Assert.That(instance.VoidReturnInvokeCount, Is.EqualTo(2));

        }

        [Test]
        public void GeneralScenarioUsingStaticRetrySyntax()
        {
            var policy = new RetryPolicy { MaxRetries = 20 };

            var instance = new TestObject();

            var result = Retry.Using(policy).Invoke(() => instance.IntReturningMethod(1, "func invoked"));

            Assert.That(result, Is.EqualTo(1));
            Assert.That(instance.IntResult, Is.EqualTo(1));
            Assert.That(instance.StringResult, Is.EqualTo("func invoked"));
        }


        private class TestObject
        {
            public string StringResult;
            public int IntResult;
            public int IntReturnInvokeCount;
            public int VoidReturnInvokeCount;


            public int IntReturningMethod(int theInt, string theString)
            {
                IntReturnInvokeCount++;
                SetProperties(theInt, theString);
                return theInt;
            }

            private void SetProperties(int theInt, string theString)
            {
                StringResult = theString;
                IntResult = theInt;
            }

            public void VoidReturningMethodThatThrowsOnFirstInvocation(int theInt, string theString)
            {

                if (VoidReturnInvokeCount == 0)
                {
                    VoidReturnInvokeCount++;
                    throw new ExpectableTestException();
                }

                VoidReturnInvokeCount++;
                SetProperties(theInt, theString);
            }
        }
    }
}
