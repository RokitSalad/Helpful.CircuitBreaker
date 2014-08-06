using System;

namespace Helpful.CircuitBreaker.Exceptions
{
    public class CircuitBreakerExecutionErrorException : CircuitBreakerException
    {
        public CircuitBreakerExecutionErrorException(CircuitBreakerConfig config, Exception exception) : base(config, exception)
        {
        }
    }
}