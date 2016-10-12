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
using NUnit.Framework;

namespace Proteus.Retry.Test
{
    [TestFixture]
    public class WhenFormattingMethodCallForLogging
    {
        [Test]
        public void CanFormatFuncWithParams()
        {
            var testObject = new TestObject();

            var result = MethodCallNameFormatter.GetFormattedName<Func<string>>(() => testObject.FuncThatTakesAnIntAndAString(1, "hello"));
            Assert.That(result, Is.EqualTo("Proteus.Retry.Test.WhenFormattingMethodCallForLogging+TestObject.FuncThatTakesAnIntAndAString(1, \"hello\")"));
        }

        [Test]
        public void CanFormatActionWithParams()
        {
            var testObject = new TestObject();

            var result = MethodCallNameFormatter.GetFormattedName<Action>(() => testObject.ActionThatTakesADoubleAndATestObject(1, new TestObject()));
            Assert.That(result, Is.EqualTo("Proteus.Retry.Test.WhenFormattingMethodCallForLogging+TestObject.ActionThatTakesADoubleAndATestObject(1, new TestObject())"));
        }

        [Test]
        public void CanFormatExpressionWithNoParams()
        {
            var testObject = new TestObject();

            var result = MethodCallNameFormatter.GetFormattedName<Action>(() => testObject.MethodWithNoParams());
            Assert.That(result, Is.EqualTo("Proteus.Retry.Test.WhenFormattingMethodCallForLogging+TestObject.MethodWithNoParams()"));
        }

        public class TestObject
        {
            public string FuncThatTakesAnIntAndAString(int i, string s)
            {
                return "";
            }

            public void MethodWithNoParams()
            { }

            public void ActionThatTakesADoubleAndATestObject(double d, TestObject obj)
            { }
        }
    }
}