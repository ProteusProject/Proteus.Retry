using System;
using System.Collections.Generic;

namespace Proteus.Retry
{
    public interface IRetryPolicy
    {
        int MaxRetries { get; set; }
        IEnumerable<Type> RetriableExceptions { get; }
        void RetryOnException<TException>() where TException : Exception;
        bool IsRetriableException<TException>() where TException : Exception;
        bool IsRetriableException(Exception exception);
    }
}