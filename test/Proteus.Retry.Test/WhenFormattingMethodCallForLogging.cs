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