namespace Helpful.CircuitBreaker
{
    using System;

    /// <summary>
    /// 
    /// </summary>
    public interface IRetryScheduler
    {
        /// <summary>
        /// Gets a value indicating whether [allow retry].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow retry]; otherwise, <c>false</c>.
        /// </value>
        bool AllowRetry { get; }
        /// <summary>
        /// Begins the next period.
        /// </summary>
        /// <param name="seedUtc">The seed UTC.</param>
        void BeginNextPeriod(DateTime seedUtc);
        /// <summary>
        /// Resets this instance.
        /// </summary>
        void Reset();
    }
}