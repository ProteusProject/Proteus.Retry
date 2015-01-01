using System;

namespace Proteus.Retry.Exceptions
{
    public class MaxRetryDurationExpiredException : RetryException
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