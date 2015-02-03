using Helpful.CircuitBreaker.Config;

namespace Helpful.CircuitBreaker.Exceptions
{
    /// <summary>
    /// Thrown when the ActionResult enum value returned after execution is not ActionResult.Good
    /// </summary>
    public class ActionResultNotGoodException : CircuitBreakerException
    {
        /// <summary>
        /// Standard CircuitBreakerException constructor
        /// </summary>
        /// <param name="config"></param>
        public ActionResultNotGoodException(CircuitBreakerConfig config) : base(config)
        {
        }
    }
}