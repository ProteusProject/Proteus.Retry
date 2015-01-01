using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Proteus.Retry.Exceptions;

namespace Proteus.Retry
{
    public class Retry : IManageRetryPolicy
    {
        private readonly IManageRetryPolicy _policy;
        private readonly IList<Exception> _innerExceptionHistory = new List<Exception>();
        private bool _timerExpired;

        public Retry()
        {
            _policy = new RetryPolicy();
        }

        public Retry(IManageRetryPolicy policy)
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
            var retryCount = 0;

            _timerExpired = false;

            Timer timer = null;

            try
            {
                //if the timer-dependent value has been set, create the timer (start automatically)...
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
                        }
                        else
                        {
                            ((Action)@delegate).Invoke();

                            //this line needed to keep the compiler happy; calling code should NOT inspect the returnValue b/c its meaningless when
                            // delegate is Action (and so has no return result to expose to the calling context)
                            returnValue = default(TReturn);
                        }

                        //after _any_ successful invocation of the action, bail out of the for-loop
                        return;
                    }
                    catch (Exception exception)
                    {
                        if (IsRetriableException(exception))
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
                        throw new MaxRetryDurationExpiredException();
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
            get { return _policy.MaxRetries; }
            set { _policy.MaxRetries = value; }
        }

        public IEnumerable<Type> RetriableExceptions
        {
            get { return _policy.RetriableExceptions; }
        }

        public TimeSpan MaxRetryDuration
        {
            get { return _policy.MaxRetryDuration; }
            set { _policy.MaxRetryDuration = value; }
        }

        public void RetryOnException<TException>() where TException : Exception
        {
            _policy.RetryOnException<TException>();
        }

        public bool IsRetriableException<TException>() where TException : Exception
        {
            return _policy.IsRetriableException<TException>();
        }

        public bool IsRetriableException(Exception exception)
        {
            return _policy.IsRetriableException(exception);
        }

        #endregion
    }
}