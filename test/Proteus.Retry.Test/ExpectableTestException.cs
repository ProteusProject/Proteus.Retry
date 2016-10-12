using System;
using System.Runtime.Serialization;

namespace Proteus.Retry.Test
{
    public class ExpectableTestException : Exception
    {
        public ExpectableTestException()
        {
        }

        public ExpectableTestException(string message) : base(message)
        {
        }

        protected ExpectableTestException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ExpectableTestException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}