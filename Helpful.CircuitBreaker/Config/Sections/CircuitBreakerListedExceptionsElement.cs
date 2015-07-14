using System.Configuration;

namespace Helpful.CircuitBreaker.Config.Sections
{
    public class CircuitBreakerListedExceptionsElement : ConfigurationElement
    {
        [ConfigurationProperty("ExceptionType", IsRequired = true, IsKey = true)]
        public string ExceptionType
        {
            get { return (string)base["ExceptionType"]; }
            set { base["ExceptionType"] = value; }
        }
    }
}
