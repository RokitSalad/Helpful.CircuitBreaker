namespace Helpful.CircuitBreaker.Exceptions
{
    public class CircuitBreakerOpenException : CircuitBreakerException
    {
        public CircuitBreakerOpenException(CircuitBreakerConfig config) : base(config)
        {
        }
    }
}