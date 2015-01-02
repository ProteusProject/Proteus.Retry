using System;
using System.Collections.Generic;

namespace Proteus.Retry
{
    public interface IManageRetryPolicy
    {
        int MaxRetries { get; set; }
        IEnumerable<Type> RetriableExceptions { get; }
        TimeSpan MaxRetryDuration { get; set; }
        void RetryOnException<TException>() where TException : Exception;
        void RetryOnExceptions(IEnumerable<Type> exceptions );
        bool IsRetriableException<TException>() where TException : Exception;
        bool IsRetriableException(Exception exception);
    }
}