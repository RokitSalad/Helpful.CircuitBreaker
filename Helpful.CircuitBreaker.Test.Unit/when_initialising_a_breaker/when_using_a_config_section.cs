using System;
using System.Collections.Generic;
using System.Configuration;
using Helpful.BDD;
using Helpful.CircuitBreaker.Config;
using Helpful.CircuitBreaker.Config.Sections;
using NUnit.Framework;

namespace Helpful.CircuitBreaker.Test.Unit.when_initialising_a_breaker
{
    class when_using_a_config_section : TestBase
    {
        private CircuitBreakerConfigurationSection _circuitBreakerConfigurationSection;
        private Exception _caughtException;
        private Dictionary<string, CircuitBreakerConfig> _breakerConfigs;

        protected override void Given()
        {
            _breakerConfigs = new Dictionary<string, CircuitBreakerConfig>();
            _circuitBreakerConfigurationSection = ConfigurationManager.GetSection("circuitBreakerConfiguration") as CircuitBreakerConfigurationSection;
        }

        protected override void When()
        {
            try
            {
                foreach (CircuitBreakerConfigSection config in _circuitBreakerConfigurationSection.CircuitBreakers)
                {
                    _breakerConfigs.Add(config.AppliedToConcreteType, config.ToBreakerConfig());
                }
            }
            catch (Exception ex)
            {
                _caughtException = ex;
            }
        }

        [Then]
        public void there_are_no_exceptions()
        {
            Assert.That(_caughtException, Is.Null);
        }

        [Then]
        public void there_are_three_breaker_configs()
        {
            Assert.That(_breakerConfigs.Count, Is.EqualTo(3));
        }

        [Then]
        public void all_target_classes_have_configs()
        {
            Assert.That(
                _breakerConfigs.ContainsKey(
                    "Helpful.CircuitBreaker.Test.Unit.Resources.Dummy1, Helpful.CircuitBreaker.Test.Unit"), Is.True);
            Assert.That(
                _breakerConfigs.ContainsKey(
                    "Helpful.CircuitBreaker.Test.Unit.Resources.Dummy2, Helpful.CircuitBreaker.Test.Unit"), Is.True);
            Assert.That(
                _breakerConfigs.ContainsKey(
                    "Helpful.CircuitBreaker.Test.Unit.Resources.Dummy3, Helpful.CircuitBreaker.Test.Unit"), Is.True);
        }

        [Then]
        public void open_event_tollerance_is_set_correctly()
        {
            Assert.That(_breakerConfigs["Helpful.CircuitBreaker.Test.Unit.Resources.Dummy1, Helpful.CircuitBreaker.Test.Unit"].OpenEventTolerance, Is.EqualTo(0));
            Assert.That(_breakerConfigs["Helpful.CircuitBreaker.Test.Unit.Resources.Dummy2, Helpful.CircuitBreaker.Test.Unit"].OpenEventTolerance, Is.EqualTo(3));
            Assert.That(_breakerConfigs["Helpful.CircuitBreaker.Test.Unit.Resources.Dummy3, Helpful.CircuitBreaker.Test.Unit"].OpenEventTolerance, Is.EqualTo(0));
        }
    }
}
