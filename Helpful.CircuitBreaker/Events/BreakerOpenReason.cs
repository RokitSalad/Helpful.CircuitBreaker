namespace Helpful.CircuitBreaker.Events
{
    /// <summary>
    /// Reason the breaker is opened
    /// </summary>
    public enum BreakerOpenReason
    {
        /// <summary>
        /// Due to timeout set by the circuit breaker config
        /// </summary>
        Timeout,
        /// <summary>
        /// Due to any other exception
        /// </summary>
        Exception
    }
}