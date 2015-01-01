using System;

namespace Proteus.Retry.Exceptions
{
    public class MaxRetryDurationExpiredException : RetryInvocationFailureException
    {
        public MaxRetryDurationExpiredException()
        {
        }

        public MaxRetryDurationExpiredException(string message) : base(message)
        {
        }

        public MaxRetryDurationExpiredException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}