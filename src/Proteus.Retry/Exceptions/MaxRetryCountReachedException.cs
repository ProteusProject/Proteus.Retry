using System;
using System.Collections.Generic;

namespace Proteus.Retry.Exceptions
{
    public class MaxRetryCountReachedException : RetryException
    {
        public MaxRetryCountReachedException()
        {
        }

        public MaxRetryCountReachedException(string message) : base(message)
        {
        }

        public MaxRetryCountReachedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public IEnumerable<Exception> InnerExceptionHistory { get; set; }
    }
}