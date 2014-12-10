using System;
using Helpful.CircuitBreaker.Config;

namespace Helpful.CircuitBreaker.Schedulers
{
    /// <summary>
    /// 
    /// </summary>
    public class SequentialRetryScheduler : IRetryScheduler
    {
        private readonly object _lock = new object();
        private readonly int[] _sequence;

        private DateTime _seedUtc;
        private int _sequenceIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="SequentialRetryScheduler"/> class.
        /// </summary>
        /// <param name="retryScheduler">The retry scheduler.</param>
        public SequentialRetryScheduler(SequentialRetrySchedulerConfig retryScheduler)
        {
            _sequence = retryScheduler.RetrySequenceSeconds;
            Reset();
        }

        /// <summary>
        /// Gets the current retry period.
        /// </summary>
        /// <value>
        /// The current retry period.
        /// </value>
        public TimeSpan CurrentRetryPeriod
        {
            get { return TimeSpan.FromSeconds(_sequence[_sequenceIndex]); }
        }

        /// <summary>
        /// Gets the allowed retry time.
        /// </summary>
        /// <value>
        /// The allowed retry time.
        /// </value>
        public DateTime AllowedRetryTime
        {
            get { return _seedUtc.Add(CurrentRetryPeriod); }
        }

        /// <summary>
        /// Gets a value indicating whether [allow retry].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow retry]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowRetry
        {
            get { return DateTime.UtcNow >= AllowedRetryTime; }
        }

        /// <summary>
        /// Begins the next period.
        /// </summary>
        /// <param name="seedUtc">The seed UTC.</param>
        public void BeginNextPeriod(DateTime seedUtc)
        {
            lock (_lock)
            {
                _seedUtc = seedUtc;

                if (_sequenceIndex < _sequence.Length - 1)
                    _sequenceIndex++;
            }
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
            lock (_lock)
            {
                _sequenceIndex = -1;
            }
        }
    }
}