﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Proteus.Retry.Exceptions;

namespace Proteus.Retry
{
    public class Retry : IManageRetryPolicy
    {
        private readonly IManageRetryPolicy _policy;
        private readonly IList<Exception> _innerExceptionHistory = new List<Exception>();

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
                    if (IsRetriableException(exception.InnerException))
                    {
                        //swallow because we want/need to remain intact for next retry attempt
                        _innerExceptionHistory.Add(exception.InnerException);
                    }
                    else
                    {
                        throw exception.InnerException;
                    }
                }

                i++;

            } while (i <= MaxRetries);

            var maxRetryCountReachedException =
                new MaxRetryCountReachedException(
                    string.Format("Unable to successfully invoke method within {0} attempts.", _policy.MaxRetries))
                {
                    InnerExceptionHistory = _innerExceptionHistory
                };

            throw maxRetryCountReachedException;
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