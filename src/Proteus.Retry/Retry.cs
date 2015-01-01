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
            //necessary evil to keep the compiler happy
            // WARNING: don't do ANYTHING with this out param b/c its content isn't meaningful since we're invoking an Action
            // with no return value
            object returnValue; 
            DoInvoke(action, out returnValue);
        }

        private void DoInvoke<TReturn>(Delegate @delegate, out TReturn returnValue)
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
                        
                        //this line needed to keep the compiler happy; calling code should NOT inspect the returnValue b/c its meaningless when
                        // delegate is Action (and so has no return result to expose to the calling context)
                        returnValue = default(TReturn);
                    }

                    //after _any_ successful invocation of the action, bail out of the for-loop
                    return;
                }
                //delegate invoke will ALWAYS toss TargetInvocationException,
                // wrapping the underlying 'real' exception as its inner
                catch (TargetInvocationException exception)
                {
                    if (_policy.IsRetriableException(exception.InnerException))
                    {
                        //swallow because we want/need to remain intact for next retry attempt
                    }
                    else
                    {
                        throw exception.InnerException;
                    }
                }

                i++;

            } while (i <= _policy.MaxRetries);


            //we should NEVER get here, but this line is needed to keep the compiler happy
            returnValue = default(TReturn);
        }
    }
}