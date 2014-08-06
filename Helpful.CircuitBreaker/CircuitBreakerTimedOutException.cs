namespace Helpful.CircuitBreaker
{
    public class CircuitBreakerTimedOutException : CircuitBreakerException
    {
        public CircuitBreakerTimedOutException(CircuitBreakerConfig config) : base(config)
        {
        }
    }
}