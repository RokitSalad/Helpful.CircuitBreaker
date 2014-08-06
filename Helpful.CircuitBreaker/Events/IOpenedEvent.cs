using System;

namespace Helpful.CircuitBreaker.Events
{
    public interface IOpenedEvent
    {
        void RaiseEvent(CircuitBreakerConfig config, BreakerOpenReason reason, Exception thrownException);
    }
}