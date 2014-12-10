using Helpful.CircuitBreaker.Config;

namespace Helpful.CircuitBreaker.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    public class CircuitBreakerOpenException : CircuitBreakerException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CircuitBreakerOpenException"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public CircuitBreakerOpenException(CircuitBreakerConfig config) : base(config)
        {
        }
    }
}