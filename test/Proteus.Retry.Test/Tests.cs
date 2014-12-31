using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Proteus.Retry.Test
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
}
