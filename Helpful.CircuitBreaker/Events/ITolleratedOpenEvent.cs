using System;

namespace Helpful.CircuitBreaker.Events
{
    public interface ITolleratedOpenEvent
    {
        void RaiseEvent(short tolleratedOpenEventCount, CircuitBreakerConfig config, BreakerOpenReason reason, Exception thrownException);
    }
}