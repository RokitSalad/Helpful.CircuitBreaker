﻿using System;
using Helpful.BDD;
using Helpful.CircuitBreaker.Config;
using Helpful.CircuitBreaker.Exceptions;
using NUnit.Framework;

namespace Helpful.CircuitBreaker.Test.Unit.when_executing_code_via_the_breaker.when_returning_an_action_result.when_no_timeout_is_set
{
    class when_the_action_result_is_failure : using_a_mocked_event_factory
    {
        private CircuitBreakerConfig _config;
        private CircuitBreaker _circuitBreaker;
        private Exception _caughtException;

        protected override void Given()
        {
            base.Given();
            _config = new CircuitBreakerConfig
            {
                SchedulerConfig = new FixedRetrySchedulerConfig { RetryPeriodInSeconds = 10 }
            };
            _circuitBreaker = new CircuitBreaker(_config, EventFactory.Object);
        }

        protected override void When()
        {
            try
            {
                _circuitBreaker.Execute(() => ActionResult.Failure);
            }
            catch (Exception e)
            {
                _caughtException = e;
            }
        }

        [Then]
        public void an_exception_should_be_thrown()
        {
            Assert.That(_caughtException, Is.Not.Null);
        }

        [Then]
        public void the_inner_exception_should_be_of_the_correct_type()
        {
            Assert.That(_caughtException.InnerException, Is.TypeOf<ActionResultNotGoodException>());
        }
    }
}
