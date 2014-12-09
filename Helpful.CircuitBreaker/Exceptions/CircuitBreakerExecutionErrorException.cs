namespace Helpful.CircuitBreaker.Exceptions
{
    using System;
    using Config;

    /// <summary>
    /// 
    /// </summary>
    public class CircuitBreakerExecutionErrorException : CircuitBreakerException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CircuitBreakerExecutionErrorException"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="exception">The exception.</param>
        public CircuitBreakerExecutionErrorException(CircuitBreakerConfig config, Exception exception)
            : base(config, exception)
        {
        }
    }
}