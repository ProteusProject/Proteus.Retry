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
        private readonly ConstrainedTypesList<Exception> _retriableExceptions = new ConstrainedTypesList<Exception>();
        private TimeSpan _retryDelayInterval = TimeSpan.FromSeconds(0);

        public int MaxRetries
        {
            get { return _maxRetries; }
            set
            {
                ThrowOnInvalidValue(value, arg => arg >= 0, new ArgumentOutOfRangeException("value", "MaxRetries must be >= 0!"));
                _maxRetries = value;
            }
        }

        public IEnumerable<Type> RetriableExceptions
        {
            get { return new ReadOnlyCollection<Type>(_retriableExceptions); }
        }

        public TimeSpan MaxRetryDuration { get; set; }

        public bool HasMaxRetryDuration
        {
            get { return MaxRetryDuration != default(TimeSpan); }
        }
        
        public bool IgnoreInheritanceForRetryExceptions { get; set; }

        public TimeSpan RetryDelayInterval
        {
            get
            {
                return null != RetryDelayIntervalProvider
                    ? RetryDelayIntervalProvider()
                    : _retryDelayInterval;
            }
            set
            {
                _retryDelayInterval = value;
            }
        }

        public Func<TimeSpan> RetryDelayIntervalProvider { get; set; }

        private void ThrowOnInvalidValue<TValue>(TValue value, Func<TValue, bool> validator, Exception exception)
        {
            if (!validator.Invoke(value))
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
            var specificTypeMatched = _retriableExceptions.Contains(getTheType());

            if (IgnoreInheritanceForRetryExceptions)
            {
                return specificTypeMatched;
            }
            else
            {
                var typeMatchedToAncestor = _retriableExceptions.Any(registeredException => getTheType().IsSubclassOf(registeredException));
                return specificTypeMatched || typeMatchedToAncestor;
            }
        }
    }
}