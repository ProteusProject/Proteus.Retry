# Proteus.Retry

## Overview ##
`Proteus.Retry` is a .NET utility library that provides support for easily invoking methods such that they can be automatically retried on failure.  Retry behavior (including number of retries, interval between successive retries, and so on) are controlled through the definition of Retry Policies that can be applied to each invocation of your method.

## Using Proteus.Retry: A Simple Example ##
At its simplest, using Proteus.Retry involves the following steps:

1. Add the Proteus.Retry NuGet Package to your solution.
2. Create an instance of the Proteus.Retry object.
3. Apply a Retry Policy to the Retry object (if you desire to override the default Policy).
4. Use the Retry object to invoke a method on your own object.

As a simple example, assume you have your own class `MyObject` with a method `IncrementMe` as follows:

```csharp
public class MyObject
{
    public int IncrementMe (int input)
    {
        return input++;
    }
}
```

The simplest use of `Proteus.Retry` with this method would be:

```csharp
//create an instance of your object
var myObject = new MyObject();

//create an instance of Proteus.Retry
var retry = new Retry();

//invoke your method using Proteus.Retry
var result = retry.Invoke(() => myObject.IncrementMe(10));

//the value of the 'result' variable is now 11
```

Some important observations about the above code:

1. Your own method invocation is passed to the `.Invoke(...)` method of the Retry object as a `Func` or `Action`, meaning that `Proteus.Retry` is able to operate on _any_ .NET method call.
2. The return value of the `.Invoke(...)` method is the returned value from your own method (e.g., `.IncrementMe(...)`)
3. The default Retry Policy is _no retries at all_ so the above example is actually functionally-equivalent to just invoking your own method directly (and as such using Proteus.Retry as shown above actually adds little value to your project).

## Using Proteus.Retry: A More Complex Example ##
The true power of `Proteus.Retry` comes into play when making use of the options available in the Retry Policy to control the behavior of `Proteus.Retry` when it invokes your code.  Here's just a brief list of many of the properties a Retry Policy can control:

* Total number of retries
* Duration to wait between successive retries
* Max combined duration to wait for all retry attempts to complete
* List of `Exception` types that are to be considered 'retriable' for each invocation
* And More!

Each of these behaviors is controlled by interacting with a `RetryPolicy` object and then passing that to the `Retry` instance before you use it to invoke your own method.  Following is a more detailed example that demonstrates this more typical usage pattern:

```csharp
//create an instance of your object
var myObject = new MyObject();

//create an instance of a RetryPolicy object and set some of its properties
var policy = new RetryPolicy();
policy.MaxRetries = 10;
policy.MaxRetryDuration = TimeSpan.FromSeconds(30);
policy.RetryDelayInterval = TimeSpan.FromSeconds(2);
policy.RegisterRetriableException<System.IOException>();

//create an instance of Proteus.Retry and assign the RetryPolicy to it
var retry = new Retry(policy);

//declare a variable to hold the (eventual) return value from your method
int result;

try
{
    //invoke your method using Proteus.Retry
    result = retry.Invoke(() => myObject.IncrementMe(10));
}
catch (MaxRetryCountExceededException)
{
    Console.WriteLine("Did not successfully invoke method after 10 retry attempts!");
}
catch (MaxRetryDurationExpiredException)
{
    Console.WriteLine("Did not successfully invoke method after 30 seconds!");
}
catch (IOException)
{
    /*
    This catch(...) is only here for illustration purposes...
    Its IMPOSSIBLE for the code to end up here because we told our
    RetryPolicy to consdier IOException to be an exception type that
    should be caught by Proteus.Retry internally and retried
    */
}
catch (Exception)
{
    Console.WriteLine("Invocation threw an exception of some type other than IOException so we didn't retry on it!");
}

//the value of the 'result' variable is now 11
```
Some points about the above sample:

1. Note the use of the `RetryPolicy` object to achieve fine-grained control over the parameters used to retry each call to the `.IncrementMe(...)` method.  In this example, the `RetryPolicy` object is stating the following policy: _"Retry up to 10 times, pausing 2 seconds between each successive retry attempt using no more than 30 seconds to process all retries.  If you receive an IOException during a retry, consider that to be an 'expected' exception that should be retried, but if you receieve any other exception abort the retry process immediately and rethrow that execption back to the calling code."_
2. Note that the invocation of the method by `Proteus.Retry` has been wrapped in a `try...catch` block in the calling code.  The use of the multiple `catch(...)` blocks permits the calling code to handle the failure case where `Proteus.Retry` is unable to successfully invoke the method within its maximum configured number of retries or its maximum configured time limit.  The type of `Exception` returned indicates whether the eventual failure was the result of exceeding the number of permitted attempts, exceding the permitted time limit, or an unexpected Exception that the `RetryPolicy` was not configured to consider 'retriable'.

### See the full docs on the [Wiki](https://github.com/ProteusProject/Proteus.Retry/wiki) (coming soon!) for more detailed information on these and other features of Proteus.Retry. ###