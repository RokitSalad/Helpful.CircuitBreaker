using System;
using Helpful.CircuitBreaker.Config;

namespace Helpful.CircuitBreaker.Events
{
    /// <summary>
    /// Event args for the tolerated open circuit breaker event
    /// </summary>
    public class ToleratedOpenCircuitBreakerEventArgs : CircuitBreakerEventArgs
    {
        /// <summary>
        /// The reason why the breaker tried to open
        /// </summary>
        public BreakerOpenReason Reason { get; set; }

        /// <summary>
        /// The source exception
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// How many open events have been tolerated
        /// </summary>
        public short ToleratedOpenEventCount { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">The configuration for the source breaker</param>
        /// <param name="reason">The reason why the breaker tried to open</param>
        /// <param name="exception">The source exception</param>
        /// <param name="toleratedOpenEventCount">How many open events have been tolerated</param>
        public ToleratedOpenCircuitBreakerEventArgs(CircuitBreakerConfig config, BreakerOpenReason reason,
            Exception exception, short toleratedOpenEventCount) : base(config)
        {
            Reason = reason;
            Exception = exception;
            ToleratedOpenEventCount = toleratedOpenEventCount;
        }
    }
}