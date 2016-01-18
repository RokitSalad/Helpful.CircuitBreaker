using System;
using System.Threading.Tasks;
using Helpful.BDD;
using Helpful.CircuitBreaker;
using Helpful.CircuitBreaker.Config;
using Helpful.CircuitBreaker.Exceptions;
using NUnit.Framework;

namespace when_executing_async_code_via_the_breaker.when_breaker_state_is_open
{
    class when_breaker_ready_for_retry : TestBase
    {
        private CircuitBreakerConfig _config;
        private CircuitBreaker _circuitBreaker;
        private Exception _caughtException;
        private bool _tryingToCloseFired;

        protected override void Given()
        {
            base.Given();
            _config = new CircuitBreakerConfig();

            _circuitBreaker = new CircuitBreaker(_config);
            _circuitBreaker.State = BreakerState.Open;
            _circuitBreaker.TryingToCloseCircuitBreaker += (sender, args) => _tryingToCloseFired = true;
        }

        protected override void When()
        {
            try
            {
                Func<Task> action = async () =>
                {
                    await Task.Yield();
                };

                _circuitBreaker.ExecuteAsync(action).Wait();
            }
            catch (CircuitBreakerOpenException e)
            {
                _caughtException = e;
            }
        }

        [Then]
        public void the_breaker_should_try_to_close()
        {
            Assert.That(_tryingToCloseFired, Is.True);
        }

        [Then]
        public void a_circuit_breaker_open_exception_should_not_be_thrown()
        {
            Assert.That(_caughtException, Is.Null);
        }
    }
}
