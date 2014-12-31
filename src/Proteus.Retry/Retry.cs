using System;

namespace Proteus.Retry
{
    public class Retry
    {
        public TReturn Invoke<TReturn>(Func<TReturn> func)
        {
            return func.Invoke();
        }

        public void Invoke(Action action)
        {
            action.Invoke();
            action.Invoke();
        }
    }
}