using System;
using System.Collections.Generic;
using Helpful.BDD;
using Helpful.CircuitBreaker;
using Helpful.CircuitBreaker.Config;
using NUnit.Framework;

namespace when_executing_code_via_the_breaker.when_breaker_state_is_open.when_trying_to_close.when_configured_to_use_timeout
{
    class when_receiving_a_whitelisted_exception : TestBase
    {
        private CircuitBreakerConfig _config;
        private CircuitBreaker _circuitBreaker;
        private Exception _caughtException;
        private Exception _thrownException;
        private int _closedEventCount;

        protected override void Given()
        {
            base.Given();
            _config = new CircuitBreakerConfig
            {
                ExpectedExceptionListType = ExceptionListType.WhiteList,
                ExpectedExceptionList = new List<Type> { typeof (IndexOutOfRangeException) },
                OpenEventTolerance = 5,
                UseTimeout = true,
                Timeout = TimeSpan.FromSeconds(10),
            };

            _circuitBreaker = new CircuitBreaker(_config);
            _circuitBreaker.ClosedCircuitBreaker += (sender, args) => _closedEventCount++;
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
        public void the_breaker_should_close()
        {
            Assert.That(_circuitBreaker.State, Is.EqualTo(BreakerState.Closed));
        }

        [Then]
        public void the_closed_event_should_be_fired()
        {
            Assert.That(_closedEventCount, Is.EqualTo(1));
        }

        [Then]
        public void an_exception_should_be_thrown()
        {
            Assert.That(_caughtException, Is.Not.Null);
        }

        [Then]
        public void the_exception_should_be_the_thrown_exception()
        {
            Assert.That(_caughtException, Is.InstanceOf(typeof(IndexOutOfRangeException)));
        }
    }
}
