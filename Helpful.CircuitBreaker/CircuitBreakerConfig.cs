using System;
using System.Collections.Generic;

namespace Helpful.CircuitBreaker
{
    [Serializable]
    public class CircuitBreakerConfig
    {
        public TimeSpan Timeout { get; set; }
        public bool UseTimeout { get; set; }
        public ExceptionListType ExpectedExceptionListType { get; set; }
        public List<Type> ExpectedExceptionList { get; set; }
        public short OpenEventTolerance { get; set; }
        public int RetryPeriodInSeconds { get; set; }

        public CircuitBreakerConfig()
        {
            ExpectedExceptionList = new List<Type>();
        }
    }
}