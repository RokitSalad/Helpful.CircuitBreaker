namespace Helpful.CircuitBreaker.Events
{
    public interface IClosedEvent
    {
        void RaiseEvent(CircuitBreakerConfig config);
    }
}