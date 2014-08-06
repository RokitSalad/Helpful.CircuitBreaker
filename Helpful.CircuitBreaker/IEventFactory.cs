namespace Helpful.CircuitBreaker
{
    public interface IEventFactory
    {
        IClosedEvent GetClosedEvent();
        IOpenedEvent GetOpenedEvent();
        ITriedToCloseEvent GetTriedToCloseEvent();
        IUnregisterBreakerEvent GetUnregisterBreakerEvent();
        IRegisterBreakerEvent GetRegisterBreakerEvent();
    }
}