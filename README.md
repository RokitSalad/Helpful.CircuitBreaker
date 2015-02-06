# Helpful.CircuitBreaker #

[Nuget](https://www.nuget.org/packages/Helpful.CircuitBreaker/)

```powershell
Install-Package Helpful.CircuitBreaker
```

## Quick Start ##
The following code will initialise a basic circuit breaker which once open will not try to close until 1 minute has passed.
```c#
CircuitBreakerConfig config = new CircuitBreakerConfig
{
    BreakerId = "Some unique and constant identifier that indicates the running instance and executing process"
    SchedulerConfig = new FixedRetrySchedulerConfig { RetryPeriodInSeconds = 60 }
};
CircuitBreaker circuitBreaker = new CircuitBreaker(config);
```

To inject a circuit breaker into class TargetClass using Ninject, try code similar to this:
```c#
Bind<ICircuitBreaker>().ToMethod(c => new CircuitBreaker(new CircuitBreakerConfig
{
    BreakerId = string.Format("{0}-{1}-{2}", "Your breaker name", "TargetClass", Environment.MachineName),
    SchedulerConfig = new FixedRetrySchedulerConfig
    {
        RetryPeriodInSeconds = 60
    }
})).WhenInjectedInto(typeof(TargetClass)).InSingletonScope();
```
The above code will reuse the same breaker for all instances of the given class, so the breaker continues to report state continuously across different threads. When opened by one use, all instances of TargetClass will have an open breaker.

## Usage ##
There are 2 primary ways that the circuit breaker can be used:
<ol>
    <li>Exceptions thrown from the code you wish to break on can trigger the breaker to open.</li>
    <li>A returned value from the code you wish to break on can trigger the breaker to open.</li>
</ol>

Here are some basic examples of each scenario.

In the following example, exceptions thrown from _client.Send(request) will cause the circuit breaker to react based on the injected configuration.
```c#
public class MakeProtectedCall
{
    private ICircuitBreaker _breaker;
    private ISomeServiceClient _client;

    public MakeProtectedCall(ICircuitBreaker breaker, ISomeServiceClient client)
    {
        _breaker = breaker;
        _client = client;
    }

    public Response ExecuteCall(Request request)
    {
    	Response response = null;
        _breaker.Execute(() => response = _client.Send(request));
        return response;
    }
}
```

In the following example, exceptions thrown by _client.Send(request) will still trigger the exception handling logic of the breaker, but the lamda applies additional logic to examine the response and trigger the breaker without ever receiving an exception. This is particularly useful when using an HTTP based client that may return failures as error codes and strings instead of thrown exceptions.
```c#
public class MakeProtectedCall
{
    private ICircuitBreaker _breaker;
    private ISomeServiceClient _client;

    public MakeProtectedCall(ICircuitBreaker breaker, ISomeServiceClient client)
    {
        _breaker = breaker;
        _client = client;
    }

    public Response ExecuteCall(Request request)
    {
    	Response response = null;
        _breaker.Execute(() => {
        	response = _client.Send(request));
        	return response.Status == "OK" ? ActionResponse.Good : ActionResult.Failure;
        }
        return response;
    }
}
```

## Tracking Circuit Breaker State ##

The suggested method for tracking the state of the circuit breaker is to handle the breaker events. These are defined on the CircuitBreaker class as:
```c#
/// <summary>
/// Raised when the circuit breaker enters the closed state
/// </summary>
public event EventHandler<CircuitBreakerEventArgs> ClosedCircuitBreaker;

/// <summary>
/// Raised when the circuit breaker enters the opened state
/// </summary>
public event EventHandler<OpenedCircuitBreakerEventArgs> OpenedCircuitBreaker;

/// <summary>
/// Raised when trying to close the circuit breaker
/// </summary>
public event EventHandler<CircuitBreakerEventArgs> TryingToCloseCircuitBreaker;

/// <summary>
/// Raised when the breaker tries to open but remains closed due to tolerance
/// </summary>
public event EventHandler<ToleratedOpenCircuitBreakerEventArgs> ToleratedOpenCircuitBreaker;

/// <summary>
/// Raised when the circuit breaker is disposed
/// </summary>
public event EventHandler<CircuitBreakerEventArgs> UnregisterCircuitBreaker;

/// <summary>
/// Raised when a circuit breaker is first used
/// </summary>
public event EventHandler<CircuitBreakerEventArgs> RegisterCircuitBreaker;
```

Attach handlers to these events to send information about the event to a logging or monitoring system. In this way, sending state to Zabbix or logging to log4net is trivial.

