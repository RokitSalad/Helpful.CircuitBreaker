using System;
using System.Threading.Tasks;
using Helpful.BDD;
using Helpful.CircuitBreaker;
using Helpful.CircuitBreaker.Config;
using Helpful.CircuitBreaker.Exceptions;
using NUnit.Framework;

namespace when_executing_async_code_via_the_breaker.when_breaker_state_is_open
{
    class when_breaker_not_ready_for_try_to_close : TestBase
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
            _circuitBreaker.TryingToCloseCircuitBreaker += (sender, args) => _tryingToCloseFired = true;
        }

        protected override void When()
        {
            try
            {
                TriggerOpenBreaker();

                Func<Task> action = async () =>
                {
                    await Task.Yield();
                };

                _circuitBreaker.ExecuteAsync(action).Wait();
            }
            catch (AggregateException ae)
            {
                _caughtException = ae.InnerException;
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
            Assert.That(_tryingToCloseFired, Is.Not.True);
        }

        [Then]
        public void a_circuit_breaker_open_exception_should_be_thrown()
        {
            Assert.That(_caughtException, Is.Not.Null);
        }
    }
}
