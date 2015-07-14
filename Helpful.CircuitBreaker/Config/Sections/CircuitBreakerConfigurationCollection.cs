using System;
using System.Configuration;

namespace Helpful.CircuitBreaker.Config.Sections
{
    public class CircuitBreakerConfigurationCollection : ConfigurationElementCollection
    {
        public CircuitBreakerConfigSection this[string index]
        {
            get { return BaseGet(index) as CircuitBreakerConfigSection; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new CircuitBreakerConfigSection();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((CircuitBreakerConfigSection)element).BreakerId;
        }

        protected override string ElementName
        {
            get
            {
                return "CircuitBreaker";
            }
        }
        protected override bool IsElementName(string elementName)
        {
            return !String.IsNullOrEmpty(elementName) && elementName == ElementName;
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        public void Add(CircuitBreakerConfigSection section)
        {
            BaseAdd(section);
        }
    }
}
