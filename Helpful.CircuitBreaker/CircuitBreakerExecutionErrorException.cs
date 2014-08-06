using System;

namespace Helpful.CircuitBreaker
{
    public class CircuitBreakerExecutionErrorException : CircuitBreakerException
    {
        public CircuitBreakerExecutionErrorException(CircuitBreakerConfig config, Exception exception) : base(config, exception)
        {
        }
    }
}