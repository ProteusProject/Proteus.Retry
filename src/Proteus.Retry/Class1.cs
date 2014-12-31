using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proteus.Retry
{
    public class MyClass
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
