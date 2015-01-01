using System;

namespace Proteus.Retry
{
    public class Retry
    {
        private readonly RetryPolicy _policy;

        public Retry(RetryPolicy policy)
        {
            _policy = policy;
        }

        public TReturn Invoke<TReturn>(Func<TReturn> func)
        {
            TReturn returnValue;
            DoInvoke(func, out returnValue);
            return returnValue;
        }

        public void Invoke(Action action)
        {
            object returnValue;
            DoInvoke(action, out returnValue);
        }

        public void DoInvoke<TReturn>(Delegate @delegate, out TReturn returnValue)
        {
            var i = 0;

            do
            {
                try
                {
                    if (@delegate is Func<TReturn>)
                    {
                        returnValue = (TReturn)@delegate.DynamicInvoke();
                    }
                    else
                    {
                        @delegate.DynamicInvoke();
                        returnValue = default(TReturn);
                    }

                    //after any successful invocation of the action, bail out of the for-loop
                    return;
                }
                catch (Exception)
                {
                    //swallow!

                    //TODO: only swallow expected exceptions; otherwise throw (eventually)
                    //throw;
                }
                
                i++;

            } while (i <= _policy.MaxRetries);


            //should NEVER get here, but this line is needed to keep the compiler happy (for now)
            returnValue = default(TReturn);
        }
    }
}