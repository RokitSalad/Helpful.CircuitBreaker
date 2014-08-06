using System;

namespace Helpful.CircuitBreaker
{
    public interface IOpenedEvent
    {
        void RaiseEvent(CircuitBreakerConfig config, BreakerOpenReason reason, Exception thrownException);
    }
}