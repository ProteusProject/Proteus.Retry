using System;
using System.Collections.Generic;
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
            return Invoke(func, false);
        }

        public TReturn Invoke<TReturn>(Func<TReturn> func, bool ignoreInheritanceForRetryExceptions)
        {
            TReturn returnValue;
            DoInvoke(func, ignoreInheritanceForRetryExceptions, out returnValue);
            return returnValue;
        }

        public void Invoke(Action action)
        {
            Invoke(action, false);
        }

        public void Invoke(Action action, bool ignoreInheritanceForRetryExceptions)
        {
            //necessary evil to keep the compiler happy
            // WARNING: don't do ANYTHING with this out param b/c its content isn't meaningful since we're invoking an Action
            // with no return value
            object returnValue;
            DoInvoke(action, ignoreInheritanceForRetryExceptions, out returnValue);
        }

        private void DoInvoke<TReturn>(Delegate @delegate, bool ignoreInheritanceForRetryExceptions, out TReturn returnValue)
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

                do
                {
                    try
                    {
                        var func = @delegate as Func<TReturn>;

                        if (func != null)
                        {
                            returnValue = func.Invoke();

                            var returnTask = returnValue as Task;

                            if (returnTask != null)
                            {
                                if (returnTask.Status == TaskStatus.Faulted)
                                {
                                    returnTask.Exception.Handle(ex => true);
                                    throw new AggregateException(returnTask.Exception.InnerExceptions);
                                }
                            }
                        }
                        else
                        {
                            ((Action) @delegate).Invoke();

                            //this line needed to keep the compiler happy; calling code should NOT inspect the returnValue b/c its meaningless when
                            // delegate is Action (and so has no return result to expose to the calling context)
                            returnValue = default(TReturn);
                        }

                        //after _any_ successful invocation of the action, bail out of the for-loop
                        return;
                    }
                    catch (AggregateException aggregateException)
                    {
                        if (IsRetriableException(aggregateException.InnerException, ignoreInheritanceForRetryExceptions))
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
                        if (IsRetriableException(exception, ignoreInheritanceForRetryExceptions))
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