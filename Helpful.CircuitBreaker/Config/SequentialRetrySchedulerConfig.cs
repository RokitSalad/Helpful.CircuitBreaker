namespace Helpful.CircuitBreaker.Config
{
    using System;
    using Schedulers;

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class SequentialRetrySchedulerConfig : ISchedulerConfig
    {
        /// <summary>
        /// Gets or sets the retry sequence in seconds.
        /// </summary>
        /// <value>
        /// The retry sequence seconds.
        /// </value>
        public int[] RetrySequenceSeconds { get; set; }

        
        public Type ImplementationOfIRetryScheduler
        {
            get { return typeof (SequentialRetryScheduler); }
        }
    }
}