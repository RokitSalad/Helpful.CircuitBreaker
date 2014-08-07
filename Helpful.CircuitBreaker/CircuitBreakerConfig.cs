using System;
using System.Collections.Generic;

namespace Helpful.CircuitBreaker
{
    [Serializable]
    public class CircuitBreakerConfig
    {
        public TimeSpan Timeout { get; set; }
        public bool UseTimeout { get; set; }
        public bool UseExceptionWhiteList { get; set; }
        public bool UseExceptionBlackList { get; set; }
        public List<Type> ExceptionWhiteList { get; set; }
        public List<Type> ExceptionBlackList { get; set; }
        public short OpenEventTollerance { get; set; }
    }
}