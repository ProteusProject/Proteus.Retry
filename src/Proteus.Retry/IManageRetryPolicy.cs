using System;
using System.Collections.Generic;

namespace Proteus.Retry
{
    public interface IManageRetryPolicy
    {
        int MaxRetries { get; set; }
        IEnumerable<Type> RetriableExceptions { get; }
        TimeSpan MaxRetryDuration { get; set; }
        bool IgnoreInheritanceForRetryExceptions { get; set; }
        TimeSpan RetryDelayDuration { get; set; }
        void RegisterRetriableException<TException>() where TException : Exception;
        void RegisterRetriableExceptions(IEnumerable<Type> exceptions);
        bool IsRetriableException<TException>() where TException : Exception;
        bool IsRetriableException<TException>(bool ignoreInheritance) where TException : Exception;
        bool IsRetriableException(Exception exception);
        bool IsRetriableException(Exception exception, bool ignoreInheritance);
    }
}