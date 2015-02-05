using System;
using Helpful.CircuitBreaker.Config;

namespace Helpful.CircuitBreaker.Events
{
    /// <summary>
    /// Event args for opened circuit breaker events
    /// </summary>
    public class OpenedCircuitBreakerEventArgs : CircuitBreakerEventArgs
    {
        /// <summary>
        /// The reason for opening
        /// </summary>
        public BreakerOpenReason Reason { get; set; }

        /// <summary>
        /// The original exception
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">Configuration for the source breaker</param>
        /// <param name="reason">The reason for opening</param>
        /// <param name="exception">The original exception</param>
        public OpenedCircuitBreakerEventArgs(CircuitBreakerConfig config, BreakerOpenReason reason, Exception exception) : base(config)
        {
            this.Reason = reason;
            Exception = exception;
        }
    }
}