using System;

namespace Helpful.CircuitBreaker.Exceptions
{
    public class CircuitBreakerException : Exception
    {
        public CircuitBreakerConfig Config { get; private set; }

        public CircuitBreakerException(CircuitBreakerConfig config)
        {
            AddConfig(config);
        }

        protected CircuitBreakerException(CircuitBreakerConfig config, Exception innerException) : base(string.Empty, innerException)
        {
        }

        protected void AddConfig(CircuitBreakerConfig config)
        {
            Config = config;
            Data.Add("CircuitBreakerConfig", Config);
        }
    }
}