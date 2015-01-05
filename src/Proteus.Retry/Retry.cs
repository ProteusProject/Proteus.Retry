using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Proteus.Retry.Exceptions;

namespace Proteus.Retry
{
    public class Retry : IManageRetryPolicy
    {
        private readonly IList<Exception> _innerExceptionHistory = new List<Exception>();
        private bool _timerExpired;

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
            var signalDone = new AutoResetEvent(false);
            
            TReturn returnValue;
            DoInvoke(func, signalDone, out returnValue);
            
            signalDone.WaitOne();
            return returnValue;
        }

        public void Invoke(Action action)
        {
            var signalDone = new AutoResetEvent(false);

            //necessary evil to keep the compiler happy
            // WARNING: don't do ANYTHING with this out param b/c its content isn't meaningful since we're invoking an Action
            // with no return value
            object returnValue;
            DoInvoke(action, signalDone, out returnValue);

            signalDone.WaitOne();
        }

        private void DoInvoke<TReturn>(Delegate @delegate, AutoResetEvent signalDone, out TReturn returnValue)
        {
            var retryCount = 0;

            _timerExpired = false;

            Timer timer = null;

            try
            {
                //if the timer-dependent value has been set, create the timer (starts automatically)...
                if (MaxRetryDuration != default(TimeSpan))
                {
                    timer = new Timer(MaxRetryDurationExpiredCallback, null, MaxRetryDuration, TimeSpan.FromSeconds(0));
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

                        //after _any_ successful invocation of the action, bail out of the for-loop
                        signalDone.Set();
                        return;
                    }
                    catch (AggregateException aggregateException)
                    {
                        if (IsRetriableException(aggregateException.InnerException, Policy.IgnoreInheritanceForRetryExceptions))
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
                        if (IsRetriableException(exception, Policy.IgnoreInheritanceForRetryExceptions))
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
                    if (_timerExpired)
                    {
                        throw new MaxRetryDurationExpiredException(string.Format("The specified duration of {0} has expired and the invocation has been aborted.  {1} attempt(s) were made prior to aborting the effort.  Examine InnerExceptionHistory property for details re: each unsuccessful attempt.", MaxRetryDuration, retryCount));
                    }


                } while (retryCount <= MaxRetries);

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
            _timerExpired = true;
        }

        #region IManageRetryPolicy Members

        public int MaxRetries
        {
            get { return Policy.MaxRetries; }
            set { Policy.MaxRetries = value; }
        }

        public IEnumerable<Type> RetriableExceptions
        {
            get { return Policy.RetriableExceptions; }
        }

        public TimeSpan MaxRetryDuration
        {
            get { return Policy.MaxRetryDuration; }
            set { Policy.MaxRetryDuration = value; }
        }

        public bool IgnoreInheritanceForRetryExceptions
        {
            get { return Policy.IgnoreInheritanceForRetryExceptions; }
            set { Policy.IgnoreInheritanceForRetryExceptions = value; }
        }

        public TimeSpan RetryDelayInterval
        {
            get { return Policy.RetryDelayInterval; }
            set { Policy.RetryDelayInterval = value; }
        }

        public Func<TimeSpan> RetryDelayIntervalProvider
        {
            get { return Policy.RetryDelayIntervalProvider; }
            set { Policy.RetryDelayIntervalProvider = value; }
        }

        public void RegisterRetriableException<TException>() where TException : Exception
        {
            Policy.RegisterRetriableException<TException>();
        }

        public void RegisterRetriableExceptions(IEnumerable<Type> exceptions)
        {
            Policy.RegisterRetriableExceptions(exceptions);
        }

        public bool IsRetriableException<TException>() where TException : Exception
        {
            return Policy.IsRetriableException<TException>();
        }

        public bool IsRetriableException<TException>(bool ignoreInheritance) where TException : Exception
        {
            return Policy.IsRetriableException<TException>(ignoreInheritance);
        }

        public bool IsRetriableException(Exception exception)
        {
            return Policy.IsRetriableException(exception);
        }

        public bool IsRetriableException(Exception exception, bool ignoreInheritance)
        {
            return Policy.IsRetriableException(exception, ignoreInheritance);
        }

        #endregion
    }
}