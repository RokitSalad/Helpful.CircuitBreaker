namespace Helpful.CircuitBreaker.Events
{
    public interface IEventFactory
    {
        IClosedEvent GetClosedEvent();
        IOpenedEvent GetOpenedEvent();
        ITriedToCloseEvent GetTriedToCloseEvent();
        IUnregisterBreakerEvent GetUnregisterBreakerEvent();
        IRegisterBreakerEvent GetRegisterBreakerEvent();
        ITolleratedOpenEvent GetTolleratedOpenEvent();
    }
}