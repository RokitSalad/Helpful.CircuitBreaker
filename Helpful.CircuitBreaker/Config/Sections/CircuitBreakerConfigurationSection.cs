using System.Configuration;

namespace Helpful.CircuitBreaker.Config.Sections
{
    public class CircuitBreakerConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("CircuitBreakers", IsDefaultCollection = true, IsKey = false, IsRequired = true)]
        public CircuitBreakerConfigurationCollection CircuitBreakers
        {
            get { return (CircuitBreakerConfigurationCollection)base["CircuitBreakers"]; }
            set { base["CircuitBreakers"] = value; }
        }
    }
}
