namespace Helpful.CircuitBreaker
{
    /// <summary>
    /// 
    /// </summary>
    public enum ExceptionListType
    {
        /// <summary>
        /// No exception list parsing
        /// </summary>
        None,
        /// <summary>
        /// Blacklist (always open circuit breaker) on the specified exceptions
        /// </summary>
        BlackList,
        /// <summary>
        /// Whitelist (silently catch if thrown) the specified excpetions
        /// </summary>
        WhiteList
    }
}