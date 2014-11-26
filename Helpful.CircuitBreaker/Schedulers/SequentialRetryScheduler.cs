using System;

namespace Helpful.CircuitBreaker.Schedulers
{
    public class SequentialRetryScheduler : IRetryScheduler
    {
        readonly int[] _sequence;

        DateTime _seedUtc;
        int _sequenceIndex;

        readonly object _lock = new object();

        public SequentialRetryScheduler(int[] retrySequenceSeconds)
        {
            _sequence = retrySequenceSeconds;
            Reset();
        }

        public bool AllowRetry
        {
            get { return DateTime.UtcNow >= AllowedRetryTime; }
        }

        public void BeginNextPeriod(DateTime seedUtc)
        {
            lock (_lock)
            {
                _seedUtc = seedUtc;

                if (_sequenceIndex < _sequence.Length - 1)
                    _sequenceIndex++;
            }
        }

        public void Reset()
        {
            lock (_lock)
            {
                _sequenceIndex = -1;
            }
        }

        public TimeSpan CurrentRetryPeriod
        {
            get { return TimeSpan.FromSeconds(_sequence[_sequenceIndex]);}
        }

        public DateTime AllowedRetryTime
        {
            get { return _seedUtc.Add(CurrentRetryPeriod); }
        }
    }
}