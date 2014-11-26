using System;

namespace Helpful.CircuitBreaker.Schedulers
{
    public class FixedRetryScheduler : IRetryScheduler
    {
        DateTime _seedUtc;
        readonly int _retrySeconds;

        public FixedRetryScheduler(int retryPeriodInSeconds)
        {
            _retrySeconds = retryPeriodInSeconds;
        }

        public bool AllowRetry
        {
            get { return DateTime.UtcNow >= AllowedRetryTime; }
        }


        public void BeginNextPeriod(DateTime seedUtc)
        {
            _seedUtc = seedUtc;
        }


        public void Reset()
        {
        }

        public DateTime AllowedRetryTime
        {
            get { return _seedUtc.Add(TimeSpan.FromSeconds(_retrySeconds)); }
        }
    }
}