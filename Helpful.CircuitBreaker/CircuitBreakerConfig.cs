using System;
using System.Collections.Generic;
using Helpful.CircuitBreaker.Events;

namespace Helpful.CircuitBreaker
{
    [Serializable]
    public class CircuitBreakerConfig : ICircuitBreakerDefinition
    {
        public string BreakerId { get; set; }
        public TimeSpan Timeout { get; set; }
        public bool UseTimeout { get; set; }
        public ExceptionListType ExpectedExceptionListType { get; set; }
        public List<Type> ExpectedExceptionList { get; set; }
        public short OpenEventTolerance { get; set; }

        public CircuitBreakerConfig()
        {
            ExpectedExceptionList = new List<Type>();
        }
    }
}