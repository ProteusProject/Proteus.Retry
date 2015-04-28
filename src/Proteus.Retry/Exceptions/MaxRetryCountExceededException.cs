using System;
using System.Collections.Generic;

namespace Proteus.Retry.Exceptions
{
    /// <summary>
    /// Class MaxRetryCountExceededException.
    /// </summary>
    public class MaxRetryCountExceededException : RetryInvocationFailureException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaxRetryCountExceededException"/> class.
        /// </summary>
        public MaxRetryCountExceededException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaxRetryCountExceededException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public MaxRetryCountExceededException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaxRetryCountExceededException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public MaxRetryCountExceededException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}