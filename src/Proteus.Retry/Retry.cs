using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Proteus.Retry.Exceptions;

namespace Proteus.Retry
{
    public class Retry
    {
        private readonly IList<Exception> _innerExceptionHistory = new List<Exception>();

        public IManageRetryPolicy Policy { get; set; }


        public Retry()
            : this(new RetryPolicy())
        {
        }

        public Retry(IManageRetryPolicy policy)
        {
            Policy = policy;
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
            // WARNING: we don't do ANYTHING with this out param b/c its content isn't meaningful since this entire code path
            // invokes and Action (with no return value)
            object returnValue;
            DoInvoke(action, out returnValue);
        }

        private void DoInvoke<TReturn>(Delegate @delegate, out TReturn returnValue)
        {
            var retryCount = 0;

            var timerState = new TimerCallbackState();

            Timer timer = null;

            try
            {
                //if the timer-dependent value has been set, create the timer (starts automatically)...
                if (Policy.MaxRetryDuration != default(TimeSpan))
                {
                    timer = new Timer(MaxRetryDurationExpiredCallback, timerState, Policy.MaxRetryDuration, TimeSpan.FromSeconds(0));
                }

                //have to ensure that the out param is assigned _some_ value no matter what to keep the compiler happy
                // (this value is reset in the case of invoking a Func and is ignored in the case of invoking an Action)
                returnValue = default(TReturn);

                do
                {
                    try
                    {
                        var func = @delegate as Func<TReturn>;

                        if (func != null)
                        {
                            returnValue = func.Invoke();
                        }
                        else
                        {
                            ((Action)@delegate).Invoke();
                        }

                        var returnTask = returnValue as Task;

                        if (returnTask != null)
                        {
                            if (returnTask.Status == TaskStatus.Faulted)
                            {
                                //if in faulted state we have to tell the Task infrastructure we're handling ALL the exceptions
                                returnTask.Exception.Handle(ex => true);

                                //now that we've short-circuted the Task exception system, we can recompose our own and throw it
                                // so we can catch it ourselves below...
                                throw new AggregateException(returnTask.Exception.InnerException);
                            }
                        }

                        //after _any_ successful invocation of the action, bail out of the do-while loop
                        return;
                    }
                    catch (AggregateException aggregateException)
                    {
                        if (Policy.IsRetriableException(aggregateException.InnerException))
                        {
                            //swallow because we want/need to remain intact for next retry attempt
                            _innerExceptionHistory.Add(aggregateException.InnerException);
                        }
                        else
                        {
                            //if not an expected (retriable) exception, just re-throw it to calling code
                            throw;
                        }
                    }
                    catch (Exception exception)
                    {
                        if (Policy.IsRetriableException(exception))
                        {
                            //swallow because we want/need to remain intact for next retry attempt
                            _innerExceptionHistory.Add(exception);
                        }
                        else
                        {
                            //if not an expected (retriable) exception, just re-throw it to calling code
                            throw;
                        }
                    }

                    retryCount++;

                    //PCL doesn't offer Thread.Sleep so this hack will provide equivalent pause of the current thread for us ...
                    var sleepHack = new ManualResetEvent(false);
                    sleepHack.WaitOne(Policy.RetryDelayInterval);

                    //check the timer to see if expired, and throw appropriate exception if so...
                    if (timerState.DurationExceeded)
                    {
                        throw new MaxRetryDurationExpiredException(string.Format("The specified duration of {0} has expired and the invocation has been aborted.  {1} attempt(s) were made prior to aborting the effort.  Examine InnerExceptionHistory property for details re: each unsuccessful attempt.", Policy.MaxRetryDuration, retryCount));
                    }


                } while (retryCount <= Policy.MaxRetries);

                var maxRetryCountReachedException =
                    new MaxRetryCountExceededException(
                        string.Format(
                            "Unable to successfully invoke method within {0} attempts.  Examine InnerExceptionHistory property for details re: each unsuccessful attempt.",
                            retryCount))
                    {
                        InnerExceptionHistory = _innerExceptionHistory
                    };

                throw maxRetryCountReachedException;
            }
            finally
            {
                if (timer != null)
                    timer.Dispose();
            }
        }

        private void MaxRetryDurationExpiredCallback(object state)
        {
            var timerState = (TimerCallbackState) state;
            timerState.DurationExceeded = true;
        }

        private class TimerCallbackState
        {
            public bool DurationExceeded;
        }
    }
}