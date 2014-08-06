namespace Helpful.CircuitBreaker
{
    public interface IClosedEvent
    {
        void RaiseEvent(CircuitBreakerConfig config);
    }
}