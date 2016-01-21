using System;
using System.Threading.Tasks;
using Helpful.BDD;
using Helpful.CircuitBreaker;
using Helpful.CircuitBreaker.Config;
using NUnit.Framework;

namespace when_executing_async_code_via_the_breaker.when_returning_an_action_result.when_no_timeout_is_set
{
    class when_the_action_result_is_good : TestBase
    {
        private CircuitBreakerConfig _config;
        private CircuitBreaker _circuitBreaker;
        private Exception _caughtException;

        protected override void Given()
        {
            base.Given();
            _config = new CircuitBreakerConfig();
            _circuitBreaker = new CircuitBreaker(_config);
        }

        protected override void When()
        {
            Task.Run(async () =>
            {
                try
                {
                    Func<Task<ActionResult>> action = async () =>
                    {
                        await Task.Yield();
                        return ActionResult.Good;
                    };

                    await _circuitBreaker.ExecuteAsync(async () => await action());
                }
                catch (Exception e)
                {
                    _caughtException = e;
                }
            }).Wait();
        }

        [Then]
        public void no_exception_should_be_thrown()
        {
            Assert.That(_caughtException, Is.Null);
        }
    }
}
