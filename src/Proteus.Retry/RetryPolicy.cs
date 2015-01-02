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
                ThrowOnInvalidValue(value, arg => arg >= 0, new ArgumentOutOfRangeException("value", "MaxRetries must be >= 0!"));
                _maxRetries = value;
            }
        }

        public IEnumerable<Type> RetriableExceptions
        {
            get { return new ReadOnlyCollection<Type>(_retriableExceptions); }
        }

        public TimeSpan MaxRetryDuration { get; set; }
        public bool IgnoreInheritanceForRetryExceptions { get; set; }

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
            return IsRetriableException<TException>(false);
        }

        public bool IsRetriableException<TException>(bool ignoreInheritance) where TException : Exception
        {
            return DoIsRetriableException(() => typeof(TException), ignoreInheritance); 
        }

        public bool IsRetriableException(Exception exception)
        {
            return DoIsRetriableException(exception.GetType, false);
        }

        public bool IsRetriableException(Exception exception, bool ignoreInheritance)
        {
            return DoIsRetriableException(exception.GetType, ignoreInheritance);
        }

        private bool DoIsRetriableException(Func<Type> getTheType, bool ignoreInheritance)
        {
            var specificTypeMatched = _retriableExceptions.Contains(getTheType());
            var typeMatchedToAncestor = _retriableExceptions.Any(registeredException => getTheType().IsSubclassOf(registeredException));

            if (IgnoreInheritanceForRetryExceptions || ignoreInheritance)
            {
                return specificTypeMatched;
            }
            else
            {
                return specificTypeMatched || typeMatchedToAncestor;
            }
        }
    }
}