using System;
using System.Threading.Tasks;
using Helpful.BDD;
using Helpful.CircuitBreaker;
using Helpful.CircuitBreaker.Config;
using Helpful.CircuitBreaker.Exceptions;
using NUnit.Framework;

namespace when_executing_async_code_via_the_breaker
{
    class when_receiving_an_exception_not_in_the_whitelist : TestBase
    {
        private CircuitBreakerConfig _config;
        private CircuitBreaker _circuitBreaker;
        private Exception _caughtException;
        private NullReferenceException _thrownException;

        protected override void Given()
        {
            base.Given();
            _config = new CircuitBreakerConfig
            {
                ExpectedExceptionListType = ExceptionListType.WhiteList
            };
            _config.ExpectedExceptionList.Add(typeof(ArgumentNullException));
            _circuitBreaker = new CircuitBreaker(_config);
            _thrownException = new NullReferenceException();
        }

        protected override void When()
        {
            Task.Run(async () =>
            {
                try
                {
                    Func<Task> action = async () =>
                    {
                        await Task.Yield();
                        throw _thrownException;
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
            Assert.That(_caughtException.InnerException, Is.EqualTo(_thrownException));
        }

        [Then]
        public void the_breaker_should_open()
        {
            Assert.That(_circuitBreaker.State, Is.EqualTo(BreakerState.Open));
        }
    }
}
