using System;
using System.Collections.Generic;

namespace Proteus.Retry.Exceptions
{
    public class MaxRetryCountExceededException : RetryException
    {
        public MaxRetryCountExceededException()
        {
        }

        public MaxRetryCountExceededException(string message) : base(message)
        {
        }

        public MaxRetryCountExceededException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public IEnumerable<Exception> InnerExceptionHistory { get; set; }
    }
}