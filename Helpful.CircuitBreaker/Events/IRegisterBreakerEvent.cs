namespace Helpful.CircuitBreaker.Events
{
    public interface IRegisterBreakerEvent
    {
        void RaiseEvent(CircuitBreaker breaker);
    }
}