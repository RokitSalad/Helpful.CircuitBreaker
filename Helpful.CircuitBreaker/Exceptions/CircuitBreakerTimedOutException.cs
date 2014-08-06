namespace Helpful.CircuitBreaker.Exceptions
{
    public class CircuitBreakerTimedOutException : CircuitBreakerException
    {
        public CircuitBreakerTimedOutException(CircuitBreakerConfig config) : base(config)
        {
        }
    }
}