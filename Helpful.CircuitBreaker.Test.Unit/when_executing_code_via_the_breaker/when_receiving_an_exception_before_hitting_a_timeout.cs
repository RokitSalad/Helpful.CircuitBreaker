﻿using System;
using Helpful.BDD;
using Helpful.CircuitBreaker;
using Helpful.CircuitBreaker.Exceptions;
using Helpful.CircuitBreaker.Schedulers;
using Helpful.CircuitBreaker.Test.Unit;
using NUnit.Framework;

namespace when_executing_code_via_the_breaker
{
    class when_receiving_an_exception_before_hitting_a_timeout : using_a_mocked_event_factory
    {
        private CircuitBreakerConfig _config;
        private CircuitBreaker _circuitBreaker;
        private IRetryScheduler _scheduler;
        private Exception _caughtException;
        private ArgumentNullException _thrownException;

        protected override void Given()
        {
            base.Given();
            _thrownException = new ArgumentNullException();
            _config = new CircuitBreakerConfig
            {
                Timeout = TimeSpan.FromMilliseconds(1000),
                UseTimeout = true
            };
            _scheduler = new FixedRetryScheduler(10);
            _circuitBreaker = Factory.GetBreaker(_config, _scheduler);
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
        public void the_inner_exception_should_be_an_aggregate_exception()
        {
            Assert.That(_caughtException.InnerException, Is.TypeOf<AggregateException>());
        }

        [Then]
        public void the_aggregate_exception_contains_the_thrown_exception()
        {
            Assert.That((_caughtException.InnerException as AggregateException).InnerExceptions, Contains.Item(_thrownException));
        }

        [Then]
        public void the_breaker_should_be_open()
        {
            Assert.That(_circuitBreaker.State, Is.EqualTo(BreakerState.Open));
        }
    }
}
