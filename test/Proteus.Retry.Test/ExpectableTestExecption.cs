using System;
using System.Runtime.Serialization;

namespace Proteus.Retry.Test
{
    public class ExpectableTestExecption : Exception
    {
        public ExpectableTestExecption()
        {
        }

        public ExpectableTestExecption(string message) : base(message)
        {
        }

        protected ExpectableTestExecption(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ExpectableTestExecption(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}