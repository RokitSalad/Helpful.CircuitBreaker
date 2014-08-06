namespace Helpful.CircuitBreaker
{
    public interface IRegisterBreakerEvent
    {
        void RaiseEvent(CircuitBreaker breaker);
    }
}