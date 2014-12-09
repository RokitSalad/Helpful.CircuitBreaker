using System;
using Helpful.CircuitBreaker.Config;

namespace Helpful.CircuitBreaker.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class CircuitBreakerException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CircuitBreakerException"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        protected CircuitBreakerException(CircuitBreakerConfig config)
        {
            AddConfig(config);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CircuitBreakerException"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="innerException">The inner exception.</param>
        protected CircuitBreakerException(CircuitBreakerConfig config, Exception innerException)
            : base(string.Empty, innerException)
        {
            AddConfig(config);
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public CircuitBreakerConfig Config { get; private set; }

        private void AddConfig(CircuitBreakerConfig config)
        {
            Config = config;
            Data.Add("CircuitBreakerConfig", Config);
        }
    }
}