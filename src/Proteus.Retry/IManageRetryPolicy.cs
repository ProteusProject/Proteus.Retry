using System;
using System.Collections.Generic;

namespace Proteus.Retry
{
    /// <summary>
    /// Interface IManageRetryPolicy
    /// </summary>
    public interface IManageRetryPolicy
    {
        /// <summary>
        /// Gets or sets the maximum retries.
        /// </summary>
        /// <value>The maximum retries.</value>
        int MaxRetries { get; set; }
        /// <summary>
        /// Gets the retriable exceptions.
        /// </summary>
        /// <value>The retriable exceptions.</value>
        IEnumerable<Type> RetriableExceptions { get; }
        /// <summary>
        /// Gets or sets the maximum duration of the retry.
        /// </summary>
        /// <value>The maximum duration of the retry.</value>
        TimeSpan MaxRetryDuration { get; set; }
        /// <summary>
        /// Gets a value indicating whether this instance has maximum retry duration.
        /// </summary>
        /// <value><c>true</c> if this instance has maximum retry duration; otherwise, <c>false</c>.</value>
        bool HasMaxRetryDuration { get; }
        /// <summary>
        /// Gets or sets a value indicating whetherto [ignore inheritance for retry exceptions].
        /// </summary>
        /// <value><c>true</c> if [ignore inheritance for retry exceptions]; otherwise, <c>false</c>.</value>
        bool IgnoreInheritanceForRetryExceptions { get; set; }
        /// <summary>
        /// Gets or sets the retry delay interval.
        /// </summary>
        /// <value>The retry delay interval.</value>
        TimeSpan RetryDelayInterval { get; set; }
        /// <summary>
        /// Gets or sets the retry delay interval provider.
        /// </summary>
        /// <value>The retry delay interval provider.</value>
        Func<TimeSpan> RetryDelayIntervalProvider { get; set; }
        /// <summary>
        /// Gets the next retry delay interval.
        /// </summary>
        /// <returns>TimeSpan.</returns>
        TimeSpan NextRetryDelayInterval();
        /// <summary>
        /// Registers the retriable exception.
        /// </summary>
        /// <typeparam name="TException">The type of the Exception.</typeparam>
        void RegisterRetriableException<TException>() where TException : Exception;
        /// <summary>
        /// Registers the retriable exceptions.
        /// </summary>
        /// <param name="exceptions">The exceptions.</param>
        void RegisterRetriableExceptions(IEnumerable<Type> exceptions);
        /// <summary>
        /// Determines whether an Exception type is a retriable exception.
        /// </summary>
        /// <typeparam name="TException">The type of the Exception.</typeparam>
        /// <returns><c>true</c> if [is retriable exception]; otherwise, <c>false</c>.</returns>
        bool IsRetriableException<TException>() where TException : Exception;
        /// <summary>
        /// Determines whether an Exception instance is a retriable exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns><c>true</c> if [is retriable exception]; otherwise, <c>false</c>.</returns>
        bool IsRetriableException(Exception exception);
    }
}