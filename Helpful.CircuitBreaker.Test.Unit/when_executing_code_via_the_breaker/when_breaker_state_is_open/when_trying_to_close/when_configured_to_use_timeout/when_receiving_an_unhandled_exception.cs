using System;
using System.Collections.Generic;
using Helpful.BDD;
using Helpful.CircuitBreaker;
using Helpful.CircuitBreaker.Config;
using Helpful.CircuitBreaker.Exceptions;
using NUnit.Framework;

namespace when_executing_code_via_the_breaker.when_breaker_state_is_open.when_trying_to_close.when_configured_to_use_timeout
{
    class when_receiving_an_unhandled_exception : TestBase
    {
        private CircuitBreakerConfig _config;
        private CircuitBreaker _circuitBreaker;
        private Exception _caughtException;
        private Exception _thrownException;
        private int _openedEventCount;
        private bool _toleratedOpenedEventFired;

        protected override void Given()
        {
            base.Given();
            _config = new CircuitBreakerConfig
            {
                ExpectedExceptionListType = ExceptionListType.BlackList,
                ExpectedExceptionList = new List<Type> { typeof (IndexOutOfRangeException) },
                OpenEventTolerance = 5,
                UseTimeout = true,
                Timeout = TimeSpan.FromSeconds(10),
            };

            _circuitBreaker = new CircuitBreaker(_config);
            _circuitBreaker.OpenedCircuitBreaker += (sender, args) => _openedEventCount++;
            _circuitBreaker.ToleratedOpenCircuitBreaker += (sender, args) => _toleratedOpenedEventFired = true;
            _thrownException = new IndexOutOfRangeException();
            _circuitBreaker.State = BreakerState.Open;
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
        public void the_breaker_should_reopen()
        {
            Assert.That(_circuitBreaker.State, Is.EqualTo(BreakerState.Open));
        }

        [Then]
        public void the_open_event_should_be_fired()
        {
            Assert.That(_openedEventCount, Is.EqualTo(1));
        }

        [Then]
        public void no_exceptions_should_be_tolerated()
        {
            Assert.That(_toleratedOpenedEventFired, Is.False);
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
            Assert.That(_caughtException.InnerException, Is.InstanceOf(typeof(IndexOutOfRangeException)));
        }
    }
}
