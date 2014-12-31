using System;

namespace Proteus.Retry.Test
{
    public class TestObject
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

            if (VoidReturnInvokeCount ==0)
            {
                VoidReturnInvokeCount++;
                throw new Exception();
            }

            VoidReturnInvokeCount++;
            SetProperties(theInt, theString);
        }
    }
}
