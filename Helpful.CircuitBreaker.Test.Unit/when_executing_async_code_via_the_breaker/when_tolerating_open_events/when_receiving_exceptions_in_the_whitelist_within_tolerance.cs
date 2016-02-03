using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Helpful.BDD;
using Helpful.CircuitBreaker;
using Helpful.CircuitBreaker.Config;
using Helpful.CircuitBreaker.Exceptions;
using NUnit.Framework;

namespace when_executing_async_code_via_the_breaker.when_tolerating_open_events
{
    class when_receiving_exceptions_in_the_whitelist_within_tolerance : TestBase
    {
        private CircuitBreakerConfig _config;
        private CircuitBreaker _circuitBreaker;
        private List<Exception> _caughtExceptions;
        private ArgumentNullException _thrownException;
        private bool _toleratedOpenEventFired;

        protected override void Given()
        {
            base.Given();
            _caughtExceptions = new List<Exception>();
            _config = new CircuitBreakerConfig
            {
                ExpectedExceptionListType = ExceptionListType.WhiteList
            };
            _config.ExpectedExceptionList.Add(typeof(ArgumentNullException));
            _circuitBreaker = new CircuitBreaker(_config);
            _circuitBreaker.ToleratedOpenCircuitBreaker += (sender, args) => _toleratedOpenEventFired = true;
            _thrownException = new ArgumentNullException();
        }

        protected override void When()
        {
            CallExecuteAsync();
            CallExecuteAsync();
        }

        private void CallExecuteAsync()
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

                    await _circuitBreaker.ExecuteAsync(action);
                }
                catch (Exception e)
                {
                    _caughtExceptions.Add(e);
                }
            }).Wait();
        }

        [Then]
        public void an_exception_should_be_thrown_for_each_call()
        {
            Assert.That(_caughtExceptions.Count, Is.EqualTo(2));
        }

        [Then]
        public void no_exceptions_should_be_breaker_open_exceptions()
        {
            Assert.That(_caughtExceptions, Has.No.TypeOf<CircuitBreakerOpenException>());
        }

        [Then]
        public void the_caught_exceptions_should_be_the_thrown_exceptions()
        {
            Assert.That(_caughtExceptions[0], Is.TypeOf<ArgumentNullException>());
            Assert.That(_caughtExceptions[1], Is.TypeOf<ArgumentNullException>());
        }

        [Then]
        public void the_breaker_should_remain_closed()
        {
            Assert.That(_circuitBreaker.State, Is.EqualTo(BreakerState.Closed));
        }

        [Then]
        public void no_tolerated_open_events_should_be_raised()
        {
            Assert.That(_toleratedOpenEventFired, Is.False);
        }
    }
}
