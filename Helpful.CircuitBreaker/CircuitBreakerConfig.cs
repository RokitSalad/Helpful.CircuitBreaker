using System;
using System.Collections.Generic;

namespace Helpful.CircuitBreaker
{
    public class CircuitBreakerConfig
    {
        public TimeSpan Timeout { get; set; }
        public bool UseTimeout { get; set; }
        public bool UseExceptionWhiteList { get; set; }
        public bool UseExceptionBlackList { get; set; }
        public IList<Type> ExceptionWhiteList { get; set; }
        public IList<Type> ExceptionBlackList { get; set; }
        public short OpenEventTollerance { get; set; }
    }
}