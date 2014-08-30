namespace Helpful.CircuitBreaker.Events
{
    public interface IEventFactory
    {
        IClosedEvent GetClosedEvent();
        IOpenedEvent GetOpenedEvent();
        ITryingToCloseEvent GetTriedToCloseEvent();
        IUnregisterBreakerEvent GetUnregisterBreakerEvent();
        IRegisterBreakerEvent GetRegisterBreakerEvent();
        ITolleratedOpenEvent GetTolleratedOpenEvent();
    }
}