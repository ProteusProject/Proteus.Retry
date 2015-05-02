using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Proteus.Retry
{
    /// <summary>
    /// Class RetryPolicy.
    /// </summary>
    public class RetryPolicy : IManageRetryPolicy
    {
        /// <summary>
        /// The _max retries
        /// </summary>
        private int _maxRetries;
        /// <summary>
        /// The _retriable exceptions
        /// </summary>
        private readonly ConstrainedTypesList<Exception> _retriableExceptions = new ConstrainedTypesList<Exception>();
        /// <summary>
        /// The _retry delay interval
        /// </summary>
        private TimeSpan _retryDelayInterval = TimeSpan.FromSeconds(0);

        /// <summary>
        /// Gets or sets the maximum retries.
        /// </summary>
        /// <value>The maximum retries.</value>
        public int MaxRetries
        {
            get { return _maxRetries; }
            set
            {
                ThrowOnInvalidValue(value, arg => arg >= 0, new ArgumentOutOfRangeException("value", "MaxRetries must be >= 0!"));
                _maxRetries = value;
            }
        }

        /// <summary>
        /// Gets the retriable exceptions.
        /// </summary>
        /// <value>The retriable exceptions.</value>
        public IEnumerable<Type> RetriableExceptions
        {
            get { return new ReadOnlyCollection<Type>(_retriableExceptions); }
        }

        /// <summary>
        /// Gets or sets the maximum duration of the retry.
        /// </summary>
        /// <value>The maximum duration of the retry.</value>
        public TimeSpan MaxRetryDuration { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance has maximum retry duration.
        /// </summary>
        /// <value><c>true</c> if this instance has maximum retry duration; otherwise, <c>false</c>.</value>
        public bool HasMaxRetryDuration
        {
            get { return MaxRetryDuration != default(TimeSpan); }
        }

        /// <summary>
        /// Gets or sets a value indicating whetherto [ignore inheritance for retry exceptions].
        /// </summary>
        /// <value><c>true</c> if [ignore inheritance for retry exceptions]; otherwise, <c>false</c>.</value>
        public bool IgnoreInheritanceForRetryExceptions { get; set; }

        /// <summary>
        /// Gets or sets the retry delay interval.
        /// </summary>
        /// <value>The retry delay interval.</value>
        public TimeSpan RetryDelayInterval { get; set; }


        /// <summary>
        /// Gets the next retry delay interval.
        /// </summary>
        /// <returns>TimeSpan.</returns>
        public TimeSpan NextRetryDelayInterval()
        {
            if (null != RetryDelayIntervalProvider)
            {
                RetryDelayInterval = RetryDelayIntervalProvider();
            }

            return RetryDelayInterval;
        }

        /// <summary>
        /// Gets or sets the retry delay interval provider.
        /// </summary>
        /// <value>The retry delay interval provider.</value>
        public Func<TimeSpan> RetryDelayIntervalProvider { get; set; }

        /// <summary>
        /// Throws the on invalid value.
        /// </summary>
        /// <typeparam name="TValue">The type of the t value.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="validator">The validator.</param>
        /// <param name="exception">The exception.</param>
        private void ThrowOnInvalidValue<TValue>(TValue value, Func<TValue, bool> validator, Exception exception)
        {
            if (!validator.Invoke(value))
            {
                throw exception;
            }
        }

        /// <summary>
        /// Registers the retriable exception.
        /// </summary>
        /// <typeparam name="TException">The type of the Exception.</typeparam>
        public void RegisterRetriableException<TException>() where TException : Exception
        {
            _retriableExceptions.Add(typeof(TException));
        }

        /// <summary>
        /// Registers the retriable exceptions.
        /// </summary>
        /// <param name="exceptions">The exceptions.</param>
        public void RegisterRetriableExceptions(IEnumerable<Type> exceptions)
        {
            foreach (var candidate in exceptions)
            {
                _retriableExceptions.Add(candidate);
            }
        }

        /// <summary>
        /// Determines whether an Exception type is a retriable exception.
        /// </summary>
        /// <typeparam name="TException">The type of the Exception.</typeparam>
        /// <returns><c>true</c> if [is retriable exception]; otherwise, <c>false</c>.</returns>
        public bool IsRetriableException<TException>() where TException : Exception
        {
            return DoIsRetriableException(() => typeof(TException));
        }

        /// <summary>
        /// Determines whether an Exception instance is a retriable exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns><c>true</c> if [is retriable exception]; otherwise, <c>false</c>.</returns>
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

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder(base.ToString());

            var types = new ConstrainedTypesList<Exception>();

            foreach (var retriableException in RetriableExceptions)
            {
                types.Add(retriableException);
            }

            builder.AppendFormat(": MaxRetries={0}, MaxRetryDuration={1}, RetryDelayInterval={2}, IgnoreInheritanceForRetriableExceptions={3}, RetriableExceptions={4}", MaxRetries, MaxRetryDuration, RetryDelayInterval, IgnoreInheritanceForRetryExceptions, types);

            return builder.ToString();
        }
    }
}