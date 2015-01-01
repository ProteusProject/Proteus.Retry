using System;

namespace Proteus.Retry
{
    public class RetryPolicy
    {
        private int _maxRetries;

        public int MaxRetries
        {
            get { return _maxRetries; }
            set
            {
                AssertValueIsAcceptable(value, arg => arg >= 0, new ArgumentOutOfRangeException("MaxRetries", "MaxRetries must be >= 0!"));
                _maxRetries = value;
            }
        }

        private void AssertValueIsAcceptable<TValue>(TValue value, Func<TValue, bool> isValidFunc, Exception exception) 
        {
            if (!isValidFunc.Invoke(value))
            {
                throw exception;
            }
        }
    }
}