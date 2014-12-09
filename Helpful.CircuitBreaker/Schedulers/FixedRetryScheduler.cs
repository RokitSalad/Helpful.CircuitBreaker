namespace Helpful.CircuitBreaker.Schedulers
{
    using System;
    using Config;

    /// <summary>
    /// 
    /// </summary>
    public class FixedRetryScheduler : IRetryScheduler
    {
        private readonly int _retrySeconds;
        private DateTime _seedUtc;

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedRetryScheduler"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public FixedRetryScheduler(FixedRetrySchedulerConfig config)
        {
            _retrySeconds = config.RetryPeriodInSeconds;
        }

        /// <summary>
        /// Gets the allowed retry time.
        /// </summary>
        /// <value>
        /// The allowed retry time.
        /// </value>
        public DateTime AllowedRetryTime
        {
            get { return _seedUtc.Add(TimeSpan.FromSeconds(_retrySeconds)); }
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
            _seedUtc = seedUtc;
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
        }
    }
}