using System;

namespace Helpful.CircuitBreaker
{
    public interface ITolleratedOpenEvent
    {
        void RaiseEvent(short tolleratedOpenEventCount, CircuitBreakerConfig config, BreakerOpenReason reason, Exception thrownException);
    }
}