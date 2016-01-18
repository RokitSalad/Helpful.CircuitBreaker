using System;
using System.Threading.Tasks;
using Helpful.BDD;
using Helpful.CircuitBreaker;
using Helpful.CircuitBreaker.Config;
using NUnit.Framework;

namespace when_executing_async_code_via_the_breaker.when_receiving_an_exception_in_the_whitelist
{
    class when_permitted_exception_pass_through_is_set_to_pass_through : TestBase
    {
        private CircuitBreakerConfig _config;
        private CircuitBreaker _circuitBreaker;
        private Exception _caughtException;
        private ArgumentNullException _thrownException;

        protected override void Given()
        {
            base.Given();
            _config = new CircuitBreakerConfig
            {
                ExpectedExceptionListType = ExceptionListType.WhiteList
            };

            _config.ExpectedExceptionList.Add(typeof(ArgumentNullException));
            _circuitBreaker = new CircuitBreaker(_config);
            _thrownException = new ArgumentNullException();
        }

        protected override void When()
        {
            try
            {
                Func<Task> action = async () =>
                {
                    await Task.Yield();
                    throw _thrownException;
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
        public void the_exception_should_be_the_thrown_exception()
        {
            Assert.That(_caughtException, Is.EqualTo(_thrownException));
        }

        [Then]
        public void the_breaker_should_remain_closed()
        {
            Assert.That(_circuitBreaker.State, Is.EqualTo(BreakerState.Closed));
        }
    }
}
