﻿

using System;
using Helpful.BDD;
using Helpful.CircuitBreaker;
using Helpful.CircuitBreaker.Config;
using Helpful.CircuitBreaker.Exceptions;
using Helpful.CircuitBreaker.Test.Unit;
using NUnit.Framework;

namespace when_executing_code_via_the_breaker
{
    class when_receiving_an_unhandled_exception : using_a_mocked_event_factory
    {
        private CircuitBreakerConfig _config;
        private CircuitBreaker _circuitBreaker;
        private Exception _caughtException;
        private ArgumentNullException _thrownException;

        protected override void Given()
        {
            base.Given();
            _thrownException = new ArgumentNullException();
            _config = new CircuitBreakerConfig
            {
                SchedulerConfig = new FixedRetrySchedulerConfig {RetryPeriodInSeconds = 10}
            };
            _circuitBreaker = new CircuitBreaker(EventFactory.Object, _config);
        }

        protected override void When()
        {
            try
            {
                _circuitBreaker.Execute(() => { throw _thrownException; });
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
        public void the_exception_should_be_a_circuit_breaker_exception()
        {
            Assert.That(_caughtException, Is.AssignableTo<CircuitBreakerException>());
        }

        [Then]
        public void the_exception_should_be_specifically_a_execution_error_exception()
        {
            Assert.That(_caughtException, Is.AssignableTo<CircuitBreakerExecutionErrorException>());
        }

        [Then]
        public void the_inner_exception_should_be_the_thrown_exception()
        {
            Assert.That(_caughtException.InnerException, Is.EqualTo(_thrownException));
        }

        [Then]
        public void the_breaker_should_open()
        {
            Assert.That(_circuitBreaker.State, Is.EqualTo(BreakerState.Open));
        }
    }
}
