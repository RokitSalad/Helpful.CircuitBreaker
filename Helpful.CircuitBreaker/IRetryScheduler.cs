using System;

namespace Helpful.CircuitBreaker
{
    public interface IRetryScheduler
    {
        bool AllowRetry { get; }
        void BeginNextPeriod(DateTime seedUtc);
        void Reset();
    }
}