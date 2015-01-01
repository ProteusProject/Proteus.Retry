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
                AssertValueAcceptable<int, ArgumentOutOfRangeException>(value, arg => arg >= 0);
                _maxRetries = value;
            }
        }

        private void AssertValueAcceptable<TValue, TException>(TValue value, Func<TValue, bool> isValidFunc) where TException: Exception, new() 
        {
            if (!isValidFunc.Invoke(value))
            {
                throw new TException();
            }
        }
    }
}