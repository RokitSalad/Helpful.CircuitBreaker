﻿using System;
using Helpful.BDD;
using Helpful.CircuitBreaker;
using Helpful.CircuitBreaker.Config;
using Helpful.CircuitBreaker.Test.Unit;
using NUnit.Framework;

namespace when_executing_code_via_the_breaker.when_returning_an_action_result.when_a_timeout_is_set
{
    class when_the_action_result_is_good : using_a_mocked_event_factory
    {
        private CircuitBreakerConfig _config;
        private CircuitBreaker _circuitBreaker;
        private TimeSpan _timeout;
        private Exception _caughtException;

        protected override void Given()
        {
            base.Given();
            _timeout = TimeSpan.FromSeconds(10);
            _config = new CircuitBreakerConfig
            {
                SchedulerConfig = new FixedRetrySchedulerConfig { RetryPeriodInSeconds = 10 },
                UseTimeout = true,
                Timeout = _timeout
            };
            _circuitBreaker = new CircuitBreaker(EventFactory.Object, _config);
        }

        protected override void When()
        {
            try
            {
                _circuitBreaker.Execute(() => ActionResult.Good);
            }
            catch (Exception e)
            {
                _caughtException = e;
            }
        }

        [Then]
        public void no_exception_should_be_thrown()
        {
            Assert.That(_caughtException, Is.Null);
        }
    }
}