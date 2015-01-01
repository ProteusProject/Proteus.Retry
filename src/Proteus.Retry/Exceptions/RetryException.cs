using System;

namespace Proteus.Retry.Exceptions
{
    public class RetryException : Exception
    {
        public RetryException()
        {
        }

        public RetryException(string message) : base(message)
        {
        }

        public RetryException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}