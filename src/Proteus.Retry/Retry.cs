using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Proteus.Retry.Exceptions;

namespace Proteus.Retry
{
    /// <summary>
    /// Class Retry.
    /// </summary>
    public class Retry
    {
        private readonly IList<Exception> _innerExceptionHistory = new List<Exception>();

        /// <summary>
        /// Gets or sets the Retry Policy.
        /// </summary>
        /// <value>The policy.</value>
        public IManageRetryPolicy Policy { get; set; }
        /// <summary>
        /// Sets the logger.
        /// </summary>
        /// <value>The logger.</value>
        public ILog Logger { private get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Retry"/> class.
        /// </summary>
        public Retry()
            : this(new RetryPolicy())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Retry"/> class.
        /// </summary>
        /// <param name="policy">The retry policy.</param>
        public Retry(IManageRetryPolicy policy)
        {
            Policy = policy;
            Logger = new InternalNoOpLogger();
        }


        /// <summary>
        /// Invokes the specified function, respecting the current retry policy.
        /// </summary>
        /// <typeparam name="TReturn">The type of the return value from the function.</typeparam>
        /// <param name="func">The function to invoke.</param>
        /// <returns>TReturn.</returns>
        public TReturn Invoke<TReturn>(Func<TReturn> func)
        {
            Logger.DebugFormat("Invoking Func {0}", func);

            TReturn returnValue;
            DoInvoke(func, out returnValue);

            return returnValue;
        }

        /// <summary>
        /// Invokes the specified action, respecting the current retry policy.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        public void Invoke(Action action)
        {
            Logger.DebugFormat("Invoking Action {0}", action);

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
                //if the timer-dependent value has been specified, create the timer (starts automatically)...
                if (Policy.HasMaxRetryDuration)
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
                        throw new MaxRetryDurationExpiredException(string.Format("The specified duration of {0} has expired and the invocation has been aborted.  {1} attempt(s) were made prior to aborting the effort.  Examine InnerExceptionHistory property for details re: each unsuccessful attempt.", Policy.MaxRetryDuration, retryCount))
                        {
                            InnerExceptionHistory = _innerExceptionHistory
                        };
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
            var timerState = (TimerCallbackState)state;
            timerState.DurationExceeded = true;
        }

        private class TimerCallbackState
        {
            /// <summary>
            /// Flag indicating that the duration has been exceeded
            /// </summary>
            public bool DurationExceeded;
        }

    }
}