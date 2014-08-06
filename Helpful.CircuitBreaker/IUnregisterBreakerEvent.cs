namespace Helpful.CircuitBreaker
{
    public interface IUnregisterBreakerEvent
    {
        void RaiseEvent(CircuitBreaker breaker);
    }
}