using System;

namespace Proteus.Retry
{
    public class Retry
    {
        private readonly RetryPolicy _policy;

        public Retry(RetryPolicy policy)
        {
            _policy = policy;
        }

        public TReturn Invoke<TReturn>(Func<TReturn> func)
        {
            return func.Invoke();
        }

        public void Invoke(Action action)
        {
            for (int i = 0; i < _policy.MaxRetries; i++)
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception)
                {
                    //swallow
                }
            }
        }
    }
}