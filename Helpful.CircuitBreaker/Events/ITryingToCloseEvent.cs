namespace Helpful.CircuitBreaker.Events
{
    public interface ITryingToCloseEvent
    {
        void RaiseEvent(CircuitBreakerConfig config);
    }
}