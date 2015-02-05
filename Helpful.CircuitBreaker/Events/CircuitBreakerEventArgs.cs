using System;
using Helpful.CircuitBreaker.Config;

namespace Helpful.CircuitBreaker.Events
{
    /// <summary>
    /// Event arguments for the circuit breaker events
    /// </summary>
    public class CircuitBreakerEventArgs : EventArgs
    {
        /// <summary>
        /// The config for the source breaker
        /// </summary>
        public CircuitBreakerConfig Config { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">The config for the source breaker</param>
        public CircuitBreakerEventArgs(CircuitBreakerConfig config)
        {
            Config = config;
        }
    }
}