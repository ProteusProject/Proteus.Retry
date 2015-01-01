using System;
using System.Collections.Generic;

namespace Proteus.Retry.Exceptions
{
    public class RetryInvocationFailureException : RetryException
    {
        public RetryInvocationFailureException()
        {
        }

        public RetryInvocationFailureException(string message) : base(message)
        {
        }

        public RetryInvocationFailureException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public IEnumerable<Exception> InnerExceptionHistory { get; set; }
    }
}