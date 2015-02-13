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
    class when_breaker_not_ready_for_try_to_close : using_a_mocked_event_factory
    {
        private CircuitBreakerConfig _config;
        private CircuitBreaker _circuitBreaker;
        private Exception _caughtException;

        protected override void Given()
        {
            base.Given();
            _config = new CircuitBreakerConfig();

            _circuitBreaker = new CircuitBreaker(EventFactory.Object, _config);
        }

        protected override void When()
        {
            try
            {
                TriggerOpenBreaker();
                _circuitBreaker.Execute(() => {});
            }
            catch (CircuitBreakerOpenException e)
            {
                _caughtException = e;
            }
        }

        private void TriggerOpenBreaker()
        {
            try
            {
                _circuitBreaker.Execute(() => { throw new Exception(); });
            }
            catch (Exception)
            {
                //swallow - just allow the breaker to open
            }
        }

        [Then]
        public void the_breaker_should_not_try_to_close()
        {
            TryingToCloseEvent.Verify(e => e.RaiseEvent(_config), Times.Never);
        }

        [Then]
        public void a_circuit_breaker_open_exception_should_be_thrown()
        {
            Assert.That(_caughtException, Is.Not.Null);
        }
    }
}
