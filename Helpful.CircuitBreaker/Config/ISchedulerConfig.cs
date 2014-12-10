using System;

namespace Helpful.CircuitBreaker.Config
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISchedulerConfig
    {

        /// <summary>
        /// Gets the configuration for this implementation of  retry scheduler.
        /// Class must have single parameter constructor of the scheduler configuration
        /// </summary>
        /// <value>
        /// The configuration for this implementation of retry scheduler.
        /// </value>
        Type ImplementationOfIRetryScheduler { get; }
    }
}