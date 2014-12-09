namespace Helpful.CircuitBreaker.Exceptions
{
    using Config;

    /// <summary>
    /// 
    /// </summary>
    public class CircuitBreakerTimedOutException : CircuitBreakerException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CircuitBreakerTimedOutException"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public CircuitBreakerTimedOutException(CircuitBreakerConfig config)
            : base(config)
        {
        }
    }
}