# Helpful.CircuitBreaker #
## Quick Start ##
```c#
CircuitBreakerFactory circuitBreakerFactory = new CircuitBreakerFactory(new MyEventFactory());
```
Creates a new instance of the circuit breaker factory wiht your own implementation of the event factory.
Or use one of the prebuilt ones available here:
 - [Azure Queue Event Factory](https://github.com/RokitSalad/Helpful.CircuitBreaker.Events.AzureQueue)
 - [Azure WAD Event Factory](https://github.com/RokitSalad/Helpful.CircuitBreaker.Events.AzureWad)
 - [Eventstore Event Factory](https://github.com/RokitSalad/Helpful.CircuitBreaker.Events.EventStore)
 ```
var circuitBreakerConfig = new CircuitBreakerConfig
{
	//The Breaker ID
	BreakerId = "CircuitBreaker-Test",
	
	//The Scheduler config and strategy 
	//Fixed 
	SchedulerConfig = new FixedRetrySchedulerConfig
	{
		RetryPeriodInSeconds = 10
	},
	//or Sequential Retry
	SchedulerConfig = new SequentialRetrySchedulerConfig()
	{
		RetrySequenceSeconds = new[] { 10, 20, 30 }
	},
	// Or Implement your own 
	
	// (Optional) ExceptionListType.BlackList Always open breaker on these exceptions 
	//or ExceptionListType.WhiteList ignore / catch these exceptions 
	ExpectedExceptionListType = ExceptionListType.BlackList,

	// (Optional) The Exception List 
	ExpectedExceptionList = new List<Type> { typeof(HttpException) },

	//(Optional) Defaut to 0 The number of times an error can occur before the circuit breaker is opened
	OpenEventTolerance = 3,

	// (Optional) Timeout operations in the circuit breaker and open the circuit
	UseTimeout = false,

	// (Optional) The timeout timespan 
	Timeout = TimeSpan.FromSeconds(10),
};

//Register Circuit Breaker with id "CircuitBreaker-Test"  Also returns the circuit breaker if required
circuitBreakerFactory.RegisterBreaker(circuitBreakerConfig);

//Get Circuit Breaker with id "CircuitBreaker-Test"
CircuitBreaker cb = circuitBreakerFactory.GetBreaker("CircuitBreaker-Test");

//Execute Actions within the breaker
cb.Execute(() => Console.WriteLine("Inside Circuit Breaker"));

// Return from functions within the breaker
string result = cb.Execute(() => "Return result from inside Circuit Breaker");

//Read current state
Console.WriteLine(cb.State);
```
