﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Proteus.Retry.Test
{
    [TestFixture]
    public class GeneralUsageSample
    {
        [Test]
        public void GeneralScenario1()
        {
            var instance = new TestObject();

            var retry = new Retry(new RetryPolicy() {MaxRetries = 20});

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
                    throw new Exception();
                }

                VoidReturnInvokeCount++;
                SetProperties(theInt, theString);
            }
        }
    }
}
