using System;
using System.Reflection;

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
                catch (TargetInvocationException exception)
                {
                    if (_policy.IsRetriableException(exception.InnerException))
                    {
                        //swallow!
                    }
                    else
                    {
                        throw exception.InnerException;
                    }
                }

                i++;

            } while (i <= _policy.MaxRetries);


            //should NEVER get here, but this line is needed to keep the compiler happy
            returnValue = default(TReturn);
        }
    }
}