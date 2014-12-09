namespace Helpful.CircuitBreaker.Config
{
    using System;
    using Schedulers;

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class FixedRetrySchedulerConfig : ISchedulerConfig
    {
        /// <summary>
        /// Gets or sets the retry period in seconds.
        /// </summary>
        /// <value>
        /// The retry period in seconds.
        /// </value>
        public int RetryPeriodInSeconds { get; set; }

        /// <summary>
        /// Gets the configuration for.
        /// </summary>
        /// <value>
        /// The configuration for.
        /// </value>
        public Type ImplementationOfIRetryScheduler
        {
            get { return typeof (FixedRetryScheduler); }
        }
    }
}