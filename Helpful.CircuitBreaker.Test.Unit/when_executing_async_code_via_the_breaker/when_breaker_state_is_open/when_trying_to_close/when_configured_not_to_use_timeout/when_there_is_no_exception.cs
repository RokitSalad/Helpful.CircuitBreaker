using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Helpful.BDD;
using Helpful.CircuitBreaker;
using Helpful.CircuitBreaker.Config;
using NUnit.Framework;

namespace when_executing_async_code_via_the_breaker.when_breaker_state_is_open.when_trying_to_close.when_configured_not_to_use_timeout
{
    class when_there_is_no_exception : TestBase
    {
        private CircuitBreakerConfig _config;
        private CircuitBreaker _circuitBreaker;
        private Exception _caughtException;
        private bool _closedEventFired;

        protected override void Given()
        {
            base.Given();
            _config = new CircuitBreakerConfig
            {
                ExpectedExceptionListType = ExceptionListType.BlackList,
                ExpectedExceptionList = new List<Type> { typeof (IndexOutOfRangeException) },
                OpenEventTolerance = 5,
            };

            _circuitBreaker = new CircuitBreaker(_config) { State = BreakerState.Open };
            _circuitBreaker.ClosedCircuitBreaker += (sender, args) => _closedEventFired = true;
        }

        protected override void When()
        {
            try
            {
                Func<Task> wrapper = async () =>
                {
                    await Task.Yield();
                };

                _circuitBreaker.ExecuteAsync(wrapper).Wait();
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
            Assert.That(_closedEventFired, Is.True);
        }

        [Then]
        public void no_exception_should_be_thrown()
        {
            Assert.That(_caughtException, Is.Null);
        }
    }
}
