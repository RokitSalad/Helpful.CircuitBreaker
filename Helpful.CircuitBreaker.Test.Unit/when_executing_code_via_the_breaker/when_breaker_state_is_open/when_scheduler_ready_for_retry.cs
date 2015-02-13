using System;
using Helpful.BDD;
using Helpful.CircuitBreaker;
using Helpful.CircuitBreaker.Config;
using Helpful.CircuitBreaker.Exceptions;
using Helpful.CircuitBreaker.Test.Unit;
using Moq;
using NUnit.Framework;

namespace when_executing_code_via_the_breaker.when_breaker_state_is_open
{
    class when_breaker_ready_for_retry : using_a_mocked_event_factory
    {
        private CircuitBreakerConfig _config;
        private CircuitBreaker _circuitBreaker;
        private Exception _caughtException;

        protected override void Given()
        {
            base.Given();
            _config = new CircuitBreakerConfig();

            _circuitBreaker = new CircuitBreaker(EventFactory.Object, _config);
            _circuitBreaker.State = BreakerState.Open;
        }

        protected override void When()
        {
            try
            {
                _circuitBreaker.Execute(() => { });
            }
            catch (CircuitBreakerOpenException e)
            {
                _caughtException = e;
            }
        }

        [Then]
        public void the_breaker_should_try_to_close()
        {
            TryingToCloseEvent.Verify(e => e.RaiseEvent(_config), Times.Once);
        }

        [Then]
        public void a_circuit_breaker_open_exception_should_not_be_thrown()
        {
            Assert.That(_caughtException, Is.Null);
        }
    }
}
