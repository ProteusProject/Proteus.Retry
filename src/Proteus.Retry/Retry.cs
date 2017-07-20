#region License

/*
 * Copyright © 2014-2016 the original author or authors.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Proteus.Retry.Exceptions;

namespace Proteus.Retry
{
    /// <summary>
    /// Class Retry.
    /// </summary>
    public class Retry
    {
        private readonly IList<Exception> _innerExceptionHistory = new List<Exception>();
        private Guid _currentRetryId;

        /// <summary>
        /// Gets or sets the Retry Policy.
        /// </summary>
        /// <value>The policy.</value>
        public IManageRetryPolicy Policy { get; set; }

        /// <summary>
        /// Sets the logger.
        /// </summary>
        /// <value>The logger.</value>
        public Action<string> Logger { private get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Retry"/> class.
        /// </summary>
        public Retry()
            : this(new RetryPolicy())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Retry"/> class.
        /// </summary>
        /// <param name="policy">The retry policy.</param>
        public Retry(IManageRetryPolicy policy)
        {
            Policy = policy;
            Logger = msg => { };
        }


        /// <summary>
        /// Invokes the specified function, respecting the current retry policy.
        /// </summary>
        /// <typeparam name="TReturn">The type of the return value from the function.</typeparam>
        /// <param name="func">The function to invoke.</param>
        /// <returns>TReturn.</returns>
        public TReturn Invoke<TReturn>(Expression<Func<TReturn>> func)
        {
            _currentRetryId = Guid.NewGuid();

            var funcName = MethodCallNameFormatter.GetFormattedName(func);

            Logger($"RetryId: {_currentRetryId} - Begin invoking Func {funcName} using {Policy}");

            TReturn returnValue;
            Invoke(func.Compile(), out returnValue);

            Logger($"RetryId: {_currentRetryId} - Finished invoking Func {funcName} using {Policy}");

            return returnValue;
        }

        /// <summary>
        /// Invokes the specified action, respecting the current retry policy.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        public void Invoke(Expression<Action> action)
        {
            _currentRetryId = Guid.NewGuid();

            var actionName = MethodCallNameFormatter.GetFormattedName(action);

            Logger($"RetryId: {_currentRetryId} - Begin invoking Action {actionName} using {Policy}");

            //necessary evil to keep the compiler happy
            // WARNING: we don't do ANYTHING with this out param b/c its content isn't meaningful since this entire code path
            // invokes and Action (with no return value)
            object returnValue;
            Invoke(action.Compile(), out returnValue);

            Logger($"RetryId: {_currentRetryId} - Finished invoking Action {actionName} using {Policy}");

        }

        private void Invoke<TReturn>(Delegate @delegate, out TReturn returnValue)
        {
            var retryCount = 0;

            var timerState = new TimerCallbackState();

            Timer timer = null;

            try
            {
                //if the timer-dependent value has been specified, create the timer (starts automatically)...
                if (Policy.HasMaxRetryDuration)
                {
                    Logger($"RetryId: {_currentRetryId} - MaxRetryDuration setting detected ({Policy.MaxRetryDuration}), starting timer to track retry duration expiry.");

                    timer = new Timer(MaxRetryDurationExpiredCallback, timerState, Policy.MaxRetryDuration, TimeSpan.FromSeconds(0));
                }

                //have to ensure that the out param is assigned _some_ value no matter what to keep the compiler happy
                // (this value is reset in the case of invoking a Func and is ignored in the case of invoking an Action)
                returnValue = default(TReturn);

                do
                {
                    try
                    {
                        var func = @delegate as Func<TReturn>;

                        if (func != null)
                        {
                            Logger($"RetryId: {_currentRetryId} - Func Invocation Attempt #{retryCount + 1}");
                            returnValue = func.Invoke();
                        }
                        else
                        {
                            Logger($"RetryId: {_currentRetryId} - Action Invocation Attempt #{retryCount + 1}");

                            ((Action)@delegate).Invoke();
                        }

                        var returnTask = returnValue as Task;

                        if (returnTask != null)
                        {
                            Logger($"RetryId: {_currentRetryId} - Invocation returned a Task.");

                            if (returnTask.Status == TaskStatus.Faulted)
                            {
                                Logger($"RetryId: {_currentRetryId} - Task determined to be in FAULTED state.");

                                //if in faulted state we have to tell the Task infrastructure we're handling ALL the exceptions
                                returnTask.Exception.Handle(ex => true);

                                //now that we've short-circuited the Task exception system, we can recompose our own and throw it
                                // so we can catch it ourselves below...
                                throw new AggregateException(returnTask.Exception.InnerException);
                            }
                        }

                        //after _any_ successful invocation of the action, bail out of the do-while loop
                        return;
                    }
                    catch (AggregateException aggregateException)
                    {
                        if (Policy.IsRetriableException(aggregateException.InnerException))
                        {
                            LogRetriableExceptionDetected(aggregateException.InnerException);

                            //swallow because we want/need to remain intact for next retry attempt
                            _innerExceptionHistory.Add(aggregateException.InnerException);
                        }
                        else
                        {
                            LogNonRetriableExceptionDetected(aggregateException.InnerException);

                            //if not an expected (retriable) exception, just re-throw it to calling code
                            throw;
                        }
                    }
                    catch (Exception exception)
                    {
                        if (Policy.IsRetriableException(exception))
                        {
                            LogRetriableExceptionDetected(exception);

                            //swallow because we want/need to remain intact for next retry attempt
                            _innerExceptionHistory.Add(exception);
                        }
                        else
                        {
                            LogNonRetriableExceptionDetected(exception);

                            //if not an expected (retriable) exception, just re-throw it to calling code
                            throw;
                        }
                    }

                    retryCount++;

                    //if the caller has set a non-zero RetryDelayInterval, then let's wait for that duration
                    if (Policy.RetryDelayInterval != default(TimeSpan) || null != Policy.RetryDelayIntervalProvider)
                    {
                        var retryDelayIntervalBeforeWaiting = Policy.RetryDelayInterval;

                        Logger($"RetryId: {_currentRetryId} - Pausing before next retry attempt for Delay Interval of {retryDelayIntervalBeforeWaiting}.");

                        //PCL doesn't offer Thread.Sleep so this hack will provide equivalent pause of the current thread for us ...
                        var sleepHack = new ManualResetEvent(false);
                        sleepHack.WaitOne(Policy.NextRetryDelayInterval());

                        Logger($"RetryId: {_currentRetryId} - Delay Interval of {retryDelayIntervalBeforeWaiting} expired; resuming retry attempts.");
                    }
                    else
                    {
                        Logger($"RetryId: {_currentRetryId} - No RetryDelayInterval configured; skipping delay and retrying immediately.");
                    }


                    //check the timer to see if expired, and throw appropriate exception if so...
                    if (timerState.DurationExceeded)
                    {
                        Logger($"RetryId: {_currentRetryId} - MaxRetryDuration of {Policy.MaxRetryDuration} expired without completing invocation; throwing MaxRetryDurationExpiredException");

                        throw new MaxRetryDurationExpiredException($"The specified duration of {Policy.MaxRetryDuration} has expired and the invocation has been aborted.  {retryCount} attempt(s) were made prior to aborting the effort.  Examine InnerExceptionHistory property for details re: each unsuccessful attempt.")
                        {
                            InnerExceptionHistory = _innerExceptionHistory
                        };
                    }


                } while (retryCount <= Policy.MaxRetries);

                Logger($"RetryId: {_currentRetryId} - MaxRetries of {Policy.MaxRetries} reached without completing invocation; throwing MaxRetryCountExceededException");

                var maxRetryCountReachedException =
                    new MaxRetryCountExceededException($"Unable to successfully invoke method within {retryCount} attempts.  Examine InnerExceptionHistory property for details re: each unsuccessful attempt.")
                    {
                        InnerExceptionHistory = _innerExceptionHistory
                    };

                throw maxRetryCountReachedException;
            }
            finally
            {
                timer?.Dispose();
            }
        }

        private void LogNonRetriableExceptionDetected(Exception exception)
        {
            Logger($"RetryId: {_currentRetryId} - Exception of type {exception.GetType()} is not registered as retriable; rethrowing exception.");

        }

        private void LogRetriableExceptionDetected(Exception exception)
        {
            Logger($"RetryId: {_currentRetryId} - Exception of type {exception.GetType()} is registered as retriable, will retry.");
        }

        private void MaxRetryDurationExpiredCallback(object state)
        {
            var timerState = (TimerCallbackState)state;
            timerState.DurationExceeded = true;
        }

        private class TimerCallbackState
        {
            /// <summary>
            /// Flag indicating that the duration has been exceeded
            /// </summary>
            public bool DurationExceeded;
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            return $"{base.ToString()} using {Policy}";
        }

        /// <summary>
        /// Creates a new Retry instance using the specified policy.
        /// </summary>
        /// <param name="policy">The policy.</param>
        /// <returns>Retry.</returns>
        public static Retry Using(RetryPolicy policy)
        {
            return new Retry(policy);
        }
    }
}