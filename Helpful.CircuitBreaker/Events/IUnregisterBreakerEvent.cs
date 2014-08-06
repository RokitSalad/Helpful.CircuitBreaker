namespace Helpful.CircuitBreaker.Events
{
    public interface IUnregisterBreakerEvent
    {
        void RaiseEvent(CircuitBreaker breaker);
    }
}