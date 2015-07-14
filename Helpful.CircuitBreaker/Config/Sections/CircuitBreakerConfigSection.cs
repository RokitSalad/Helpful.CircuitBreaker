using System;
using System.Configuration;

namespace Helpful.CircuitBreaker.Config.Sections
{
    public class CircuitBreakerConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("BreakerId", IsRequired = true, IsKey = true)]
        public string BreakerId
        {
            get { return (string)base["BreakerId"]; }
            set { base["BreakerId"] = value; }
        }

        [ConfigurationProperty("OpenEventTolerance")]
        public short OpenEventTolerance
        {
            get { return (short)base["OpenEventTolerance"]; }
            set { base["OpenEventTolerance"] = value; }
        }

        [ConfigurationProperty("Timeout")]
        public string Timeout
        {
            get { return (string)base["Timeout"]; }
            set { base["Timeout"] = value; }
        }

        [ConfigurationProperty("UseTimeout")]
        public bool UseTimeout
        {
            get { return (bool)base["UseTimeout"]; }
            set { base["UseTimeout"] = value; }
        }

        [ConfigurationProperty("BreakerOpenPeriods")]
        public string BreakerOpenPeriods
        {
            get { return (string) base["BreakerOpenPeriods"]; }
            set { base["BreakerOpenPeriods"] = value; }
        }

        [ConfigurationProperty("AppliedToConcreteType", IsRequired = true)]
        public string AppliedToConcreteType
        {
            get { return (string)base["AppliedToConcreteType"]; }
            set { base["AppliedToConcreteType"] = value; }
        }

        [ConfigurationProperty("PermittedExceptionBehaviour", IsRequired = false)]
        public PermittedExceptionBehaviourConfig PermittedExceptionBehaviourConfig
        {
            get { return (PermittedExceptionBehaviourConfig)base["PermittedExceptionBehaviour"]; }
            set { base["PermittedExceptionBehaviour"] = value; }
        }

        [ConfigurationProperty("Exceptions", IsDefaultCollection = true, IsKey = false, IsRequired = false)]
        public CircuitBreakerListedExceptionsCollection Exceptions
        {
            get { return (CircuitBreakerListedExceptionsCollection)base["Exceptions"]; }
            set { base["Exceptions"] = value; }
        }
    }
}
