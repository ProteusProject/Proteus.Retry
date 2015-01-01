using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace Proteus.Retry
{
    public class RetryPolicy
    {
        private int _maxRetries;
        private readonly IList<Type> _retriableExceptions = new List<Type>();

        public int MaxRetries
        {
            get { return _maxRetries; }
            set
            {
                ThrowOnInvalidValue(value, arg => arg >= 0, new ArgumentOutOfRangeException("MaxRetries", "MaxRetries must be >= 0!"));
                _maxRetries = value;
            }
        }

        public IEnumerable<Type> RetriableExceptions
        {
            get { return new ReadOnlyCollection<Type>(_retriableExceptions); }
        }

        private void ThrowOnInvalidValue<TValue>(TValue value, Func<TValue, bool> isValidFunc, Exception exception)
        {
            if (!isValidFunc.Invoke(value))
            {
                throw exception;
            }
        }

        public void RetryOnException<TException>() where TException : Exception
        {
            _retriableExceptions.Add(typeof(TException));
        }

        public bool IsRetryException<TException>() where TException : Exception
        {
            return DoIsRetryException(() => typeof(TException));
        }

        public bool IsRetryException(Exception exception)
        {
            return DoIsRetryException(exception.GetType);
        }

        private bool DoIsRetryException(Func<Type> getTheType)
        {
            return _retriableExceptions.Contains(getTheType()) ||
                  _retriableExceptions.Any(registeredException => getTheType().IsSubclassOf(registeredException));
        }
    }
}