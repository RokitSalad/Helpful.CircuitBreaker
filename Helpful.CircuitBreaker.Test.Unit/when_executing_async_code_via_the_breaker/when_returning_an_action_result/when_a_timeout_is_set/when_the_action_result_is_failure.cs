using System;
using System.Threading.Tasks;
using Helpful.BDD;
using Helpful.CircuitBreaker;
using Helpful.CircuitBreaker.Config;
using Helpful.CircuitBreaker.Exceptions;
using NUnit.Framework;

namespace when_executing_async_code_via_the_breaker.when_returning_an_action_result.when_a_timeout_is_set
{
    class when_the_action_result_is_failure : TestBase
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
                UseTimeout = true,
                Timeout = _timeout
            };
            _circuitBreaker = new CircuitBreaker(_config);
        }

        protected override void When()
        {
            try
            {
                Func<Task<ActionResult>> action = async () =>
                {
                    await Task.Yield();
                    return ActionResult.Failure;
                };

                _circuitBreaker.ExecuteAsync(action).Wait();
            }
            catch (AggregateException ae)
            {
                _caughtException = ae.InnerException;
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
