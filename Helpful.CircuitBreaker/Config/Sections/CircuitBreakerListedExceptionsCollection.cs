using System.Configuration;

namespace Helpful.CircuitBreaker.Config.Sections
{
    public class CircuitBreakerListedExceptionsCollection : ConfigurationElementCollection
    {
        public CircuitBreakerListedExceptionsElement this[string index]
        {
            get { return BaseGet(index) as CircuitBreakerListedExceptionsElement; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new CircuitBreakerListedExceptionsElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((CircuitBreakerListedExceptionsElement)element).ExceptionType;
        }
        public void Add(CircuitBreakerListedExceptionsElement element)
        {
            BaseAdd(element);
        }

        [ConfigurationProperty("ListType", IsRequired = true)]
        public ExceptionListTypeConfig ListTypeConfig
        {
            get { return (ExceptionListTypeConfig)base["ListType"]; }
            set { base["ListType"] = value; }
        }
    }
}
