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

                    //after any successful invocation of the action, bail out of the for-loop
                    break;
                }
                catch (Exception)
                {
                    //swallow!
                    //TODO: only swallow expected exceptions; otherwise throw if unexpected
                }
            }
        }
    }
}