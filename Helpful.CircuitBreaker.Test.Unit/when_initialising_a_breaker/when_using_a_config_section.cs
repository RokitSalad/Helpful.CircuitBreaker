using System;
using System.Collections.Generic;
using System.Configuration;
using Helpful.BDD;
using Helpful.CircuitBreaker.Config;
using Helpful.CircuitBreaker.Config.Sections;
using Helpful.CircuitBreaker.Test.Unit.Resources;
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

        [Then]
        public void breaker_open_periods_are_set_correctly()
        {
            Assert.That(_breakerConfigs["Helpful.CircuitBreaker.Test.Unit.Resources.Dummy1, Helpful.CircuitBreaker.Test.Unit"].BreakerOpenPeriods[0].TotalSeconds, Is.EqualTo(30));
            Assert.That(_breakerConfigs["Helpful.CircuitBreaker.Test.Unit.Resources.Dummy1, Helpful.CircuitBreaker.Test.Unit"].BreakerOpenPeriods[1].TotalSeconds, Is.EqualTo(90));
            Assert.That(_breakerConfigs["Helpful.CircuitBreaker.Test.Unit.Resources.Dummy1, Helpful.CircuitBreaker.Test.Unit"].BreakerOpenPeriods[2].TotalSeconds, Is.EqualTo(200));
            Assert.That(_breakerConfigs["Helpful.CircuitBreaker.Test.Unit.Resources.Dummy2, Helpful.CircuitBreaker.Test.Unit"].BreakerOpenPeriods[0].TotalSeconds, Is.EqualTo(30));
            Assert.That(_breakerConfigs["Helpful.CircuitBreaker.Test.Unit.Resources.Dummy3, Helpful.CircuitBreaker.Test.Unit"].BreakerOpenPeriods[0].TotalSeconds, Is.EqualTo(60));
        }

        [Then]
        public void the_expected_exception_list_types_are_correct()
        {
            Assert.That(_breakerConfigs["Helpful.CircuitBreaker.Test.Unit.Resources.Dummy1, Helpful.CircuitBreaker.Test.Unit"].ExpectedExceptionListType, Is.EqualTo(ExceptionListType.WhiteList));
            Assert.That(_breakerConfigs["Helpful.CircuitBreaker.Test.Unit.Resources.Dummy2, Helpful.CircuitBreaker.Test.Unit"].ExpectedExceptionListType, Is.EqualTo(ExceptionListType.BlackList));
            Assert.That(_breakerConfigs["Helpful.CircuitBreaker.Test.Unit.Resources.Dummy3, Helpful.CircuitBreaker.Test.Unit"].ExpectedExceptionListType, Is.EqualTo(ExceptionListType.None));
        }

        [Then]
        public void the_exception_lists_are_correct()
        {
            List<Type> dummy1Exceptions =
                _breakerConfigs["Helpful.CircuitBreaker.Test.Unit.Resources.Dummy1, Helpful.CircuitBreaker.Test.Unit"]
                    .ExpectedExceptionList;
            Assert.That(dummy1Exceptions.Count, Is.EqualTo(1));
            Assert.That(dummy1Exceptions[0], Is.EqualTo(typeof(DummyException1)));

            List<Type> dummy2Exceptions =
                _breakerConfigs["Helpful.CircuitBreaker.Test.Unit.Resources.Dummy2, Helpful.CircuitBreaker.Test.Unit"]
                    .ExpectedExceptionList;
            Assert.That(dummy2Exceptions.Count, Is.EqualTo(2));
            Assert.That(dummy2Exceptions[0], Is.EqualTo(typeof(DummyException1)));
            Assert.That(dummy2Exceptions[1], Is.EqualTo(typeof(DummyException2)));

            List<Type> dummy3Exceptions =
                _breakerConfigs["Helpful.CircuitBreaker.Test.Unit.Resources.Dummy3, Helpful.CircuitBreaker.Test.Unit"]
                    .ExpectedExceptionList;
            Assert.That(dummy3Exceptions, Is.Empty);
        }

        [Then]
        public void the_timeouts_are_correct()
        {
            Assert.That(_breakerConfigs["Helpful.CircuitBreaker.Test.Unit.Resources.Dummy1, Helpful.CircuitBreaker.Test.Unit"].Timeout.TotalSeconds, Is.EqualTo(0));
            Assert.That(_breakerConfigs["Helpful.CircuitBreaker.Test.Unit.Resources.Dummy2, Helpful.CircuitBreaker.Test.Unit"].Timeout.TotalSeconds, Is.EqualTo(0));
            Assert.That(_breakerConfigs["Helpful.CircuitBreaker.Test.Unit.Resources.Dummy3, Helpful.CircuitBreaker.Test.Unit"].Timeout.TotalSeconds, Is.EqualTo(200));
        }

        [Then]
        public void the_use_timeout_flags_are_correct()
        {
            Assert.That(_breakerConfigs["Helpful.CircuitBreaker.Test.Unit.Resources.Dummy1, Helpful.CircuitBreaker.Test.Unit"].UseTimeout, Is.False);
            Assert.That(_breakerConfigs["Helpful.CircuitBreaker.Test.Unit.Resources.Dummy2, Helpful.CircuitBreaker.Test.Unit"].UseTimeout, Is.False);
            Assert.That(_breakerConfigs["Helpful.CircuitBreaker.Test.Unit.Resources.Dummy3, Helpful.CircuitBreaker.Test.Unit"].UseTimeout, Is.True);
        }

        [Then]
        public void the_breaker_ids_are_correct()
        {
            Assert.That(_breakerConfigs["Helpful.CircuitBreaker.Test.Unit.Resources.Dummy1, Helpful.CircuitBreaker.Test.Unit"].BreakerId.StartsWith("TestBreaker1"), Is.True);
            Assert.That(_breakerConfigs["Helpful.CircuitBreaker.Test.Unit.Resources.Dummy2, Helpful.CircuitBreaker.Test.Unit"].BreakerId.StartsWith("TestBreaker2"), Is.True);
            Assert.That(_breakerConfigs["Helpful.CircuitBreaker.Test.Unit.Resources.Dummy3, Helpful.CircuitBreaker.Test.Unit"].BreakerId.StartsWith("TestBreaker3"), Is.True);
        }

        [Then]
        public void the_permitted_exception_behaviours_are_correct()
        {
            Assert.That(_breakerConfigs["Helpful.CircuitBreaker.Test.Unit.Resources.Dummy1, Helpful.CircuitBreaker.Test.Unit"].PermittedExceptionPassThrough, Is.EqualTo(PermittedExceptionBehaviour.Swallow));
            Assert.That(_breakerConfigs["Helpful.CircuitBreaker.Test.Unit.Resources.Dummy2, Helpful.CircuitBreaker.Test.Unit"].PermittedExceptionPassThrough, Is.EqualTo(PermittedExceptionBehaviour.PassThrough));
            Assert.That(_breakerConfigs["Helpful.CircuitBreaker.Test.Unit.Resources.Dummy3, Helpful.CircuitBreaker.Test.Unit"].PermittedExceptionPassThrough, Is.EqualTo(PermittedExceptionBehaviour.PassThrough));
        }

        [Then]
        public void the_open_event_tolerance_reset_periods_are_correct()
        {
            Assert.That(_breakerConfigs["Helpful.CircuitBreaker.Test.Unit.Resources.Dummy1, Helpful.CircuitBreaker.Test.Unit"].OpenEventToleranceResetPeriod, Is.EqualTo(TimeSpan.FromMinutes(5)));
            Assert.That(_breakerConfigs["Helpful.CircuitBreaker.Test.Unit.Resources.Dummy2, Helpful.CircuitBreaker.Test.Unit"].OpenEventToleranceResetPeriod, Is.EqualTo(TimeSpan.FromMinutes(20)));
            Assert.That(_breakerConfigs["Helpful.CircuitBreaker.Test.Unit.Resources.Dummy3, Helpful.CircuitBreaker.Test.Unit"].OpenEventToleranceResetPeriod, Is.EqualTo(TimeSpan.FromMinutes(7)));
        }

        [Then]
        public void the_use_immediate_failure_retry_flags_are_correct()
        {
            Assert.That(_breakerConfigs["Helpful.CircuitBreaker.Test.Unit.Resources.Dummy1, Helpful.CircuitBreaker.Test.Unit"].UseImmediateFailureRetry, Is.EqualTo(false));
            Assert.That(_breakerConfigs["Helpful.CircuitBreaker.Test.Unit.Resources.Dummy2, Helpful.CircuitBreaker.Test.Unit"].UseImmediateFailureRetry, Is.EqualTo(true));
            Assert.That(_breakerConfigs["Helpful.CircuitBreaker.Test.Unit.Resources.Dummy3, Helpful.CircuitBreaker.Test.Unit"].UseImmediateFailureRetry, Is.EqualTo(false));
        }
    }
}
