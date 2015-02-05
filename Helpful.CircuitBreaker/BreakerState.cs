namespace Helpful.CircuitBreaker
{
    /// <summary>
    /// 
    /// </summary>
    public enum BreakerState
    {
        /// <summary>
        /// Breaker is uninitialised
        /// </summary>
        Uninitialised,
        /// <summary>
        /// Breaker is open
        /// </summary>
        Open,
        /// <summary>
        /// The breaker is closed
        /// </summary>
        Closed,
        /// <summary>
        /// The breaker half open
        /// </summary>
        HalfOpen
    }
}