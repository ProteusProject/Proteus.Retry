using System;

namespace Proteus.Retry.Exceptions
{
    /// <summary>
    /// Class MaxRetryDurationExpiredException.
    /// </summary>
    public class MaxRetryDurationExpiredException : RetryInvocationFailureException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaxRetryDurationExpiredException"/> class.
        /// </summary>
        public MaxRetryDurationExpiredException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaxRetryDurationExpiredException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public MaxRetryDurationExpiredException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaxRetryDurationExpiredException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public MaxRetryDurationExpiredException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}