using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ClassLibrary1
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void Test()
        {

            var instance = new MyClass();

            var retry = new Retry();

            var result = retry.Invoke(() => instance.IntReturningMethod(1, "func invoked"));

            Assert.That(result, Is.EqualTo(1));
            Assert.That(instance.IntResult, Is.EqualTo(1));
            Assert.That(instance.StringResult, Is.EqualTo("func invoked"));

            retry.Invoke(() => instance.VoidReturningMethod(2, "action invoked"));

            Assert.That(instance.IntResult, Is.EqualTo(2));
            Assert.That(instance.StringResult, Is.EqualTo("action invoked"));

            Assert.That(instance.IntReturnInvokeCount, Is.EqualTo(1));
            Assert.That(instance.VoidReturnInvokeCount, Is.EqualTo(2));

        }
    }


    class MyClass
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

        public void VoidReturningMethod(int theInt, string theString)
        {
            VoidReturnInvokeCount++;
            SetProperties(theInt, theString);
        }
    }

    public class Retry
    {
        public TReturn Invoke<TReturn>(Func<TReturn> func)
        {
            return func.Invoke();
        }

        public void Invoke(Action action)
        {
            action.Invoke();
            action.Invoke();
        }
    }
}
