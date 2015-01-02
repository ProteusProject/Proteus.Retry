using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace Proteus.Retry
{
    public class RetryPolicy : IManageRetryPolicy
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

        public TimeSpan MaxRetryDuration { get; set; }

        private void ThrowOnInvalidValue<TValue>(TValue value, Func<TValue, bool> isValidFunc, Exception exception)
        {
            if (!isValidFunc.Invoke(value))
            {
                throw exception;
            }
        }

        public void RegisterRetriableException<TException>() where TException : Exception
        {
            _retriableExceptions.Add(typeof(TException));
        }

        public void RegisterRetriableExceptions(IEnumerable<Type> exceptions)
        {
            foreach (var candidate in exceptions.Where(candidate => candidate.IsSubclassOf(typeof(Exception))))
            {
                _retriableExceptions.Add(candidate);
            }
        }

        public bool IsRetriableException<TException>() where TException : Exception
        {
            return DoIsRetriableException(() => typeof(TException));
        }

        public bool IsRetriableException(Exception exception)
        {
            return DoIsRetriableException(exception.GetType);
        }

        private bool DoIsRetriableException(Func<Type> getTheType)
        {
            return _retriableExceptions.Contains(getTheType()) ||
                  _retriableExceptions.Any(registeredException => getTheType().IsSubclassOf(registeredException));
        }
    }
}