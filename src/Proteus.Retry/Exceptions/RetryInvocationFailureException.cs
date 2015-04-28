using System;
using System.Collections.Generic;

namespace Proteus.Retry.Exceptions
{
    /// <summary>
    /// Class RetryInvocationFailureException.
    /// </summary>
    public class RetryInvocationFailureException : RetryException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RetryInvocationFailureException"/> class.
        /// </summary>
        public RetryInvocationFailureException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryException" /> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public RetryInvocationFailureException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryInvocationFailureException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public RetryInvocationFailureException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Gets or sets the inner exception history.
        /// </summary>
        /// <value>The inner exception history.</value>
        public IEnumerable<Exception> InnerExceptionHistory { get; set; }
    }
}