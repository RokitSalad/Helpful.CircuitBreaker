using System;
using Helpful.BDD;
using Helpful.CircuitBreaker;
using Helpful.CircuitBreaker.Config;
using NUnit.Framework;

namespace when_executing_code_via_the_breaker.when_returning_an_action_result.when_no_timeout_is_set
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
            try
            {
                _circuitBreaker.Execute(() => ActionResult.Good);
            }
            catch (Exception e)
            {
                _caughtException = e;
            }
        }

        [Then]
        public void no_exception_should_be_thrown()
        {
            Assert.That(_caughtException, Is.Null);
        }
    }
}
