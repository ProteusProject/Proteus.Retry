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

            retry.Logger = msg => LogManager.GetLogger(this.GetType()).Debug(msg);

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
