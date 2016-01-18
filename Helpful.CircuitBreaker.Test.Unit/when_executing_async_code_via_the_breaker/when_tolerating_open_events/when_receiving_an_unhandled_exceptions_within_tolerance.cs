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
    class when_receiving_an_unhandled_exceptions_within_tolerance : TestBase
    {
        private CircuitBreakerConfig _config;
        private CircuitBreaker _circuitBreaker;
        private List<Exception> _caughtExceptions;
        private ArgumentNullException _thrownException;
        private int _toleratedOpenEventCount;

        protected override void Given()
        {
            base.Given();
            _caughtExceptions = new List<Exception>();
            _thrownException = new ArgumentNullException();
            _config = new CircuitBreakerConfig
            {
                OpenEventTolerance = 2
            };
            _circuitBreaker = new CircuitBreaker(_config);
            _circuitBreaker.ToleratedOpenCircuitBreaker += (sender, args) => _toleratedOpenEventCount++;
        }

        protected override void When()
        {
            CallExecuteAsync();
            CallExecuteAsync();
        }

        private void CallExecuteAsync()
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
                _caughtExceptions.Add(ae.InnerException);
            }
            catch (Exception e)
            {
                _caughtExceptions.Add(e);
            }
        }

        [Then]
        public void an_exception_should_be_thrown_for_each_call()
        {
            Assert.That(_caughtExceptions.Count, Is.EqualTo(2));
        }

        [Then]
        public void both_exceptions_should_be_execution_errors()
        {
            Assert.That(_caughtExceptions[0], Is.TypeOf<CircuitBreakerExecutionErrorException>());
            Assert.That(_caughtExceptions[1], Is.TypeOf<CircuitBreakerExecutionErrorException>());
        }

        [Then]
        public void the_inner_exceptions_should_be_the_thrown_exception()
        {
            Assert.That(_caughtExceptions[0].InnerException, Is.EqualTo(_thrownException));
            Assert.That(_caughtExceptions[1].InnerException, Is.EqualTo(_thrownException));
        }

        [Then]
        public void the_breaker_should_remain_closed()
        {
            Assert.That(_circuitBreaker.State, Is.EqualTo(BreakerState.Closed));
        }

        [Then]
        public void two_tolerated_open_events_should_be_raised()
        {
            Assert.That(_toleratedOpenEventCount, Is.EqualTo(2));
        }
    }
}
