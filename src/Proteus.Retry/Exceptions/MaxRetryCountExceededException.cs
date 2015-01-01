using System;
using System.Collections.Generic;

namespace Proteus.Retry.Exceptions
{
    public class MaxRetryCountExceededException : RetryInvocationFailureException
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
    }
}