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
    class when_receiving_exceptions_before_hitting_timeouts_beyond_tolerance : TestBase
    {
        private CircuitBreakerConfig _config;
        private CircuitBreaker _circuitBreaker;
        private List<Exception> _caughtExceptions;
        private ArgumentNullException _thrownException;
        private bool _openedEventFired;
        private int _toleratedOpenedCount;

        protected override void Given()
        {
            base.Given();
            _caughtExceptions = new List<Exception>();
            _thrownException = new ArgumentNullException();
            _config = new CircuitBreakerConfig
            {
                Timeout = TimeSpan.FromMilliseconds(1000),
                UseTimeout = true,
                OpenEventTolerance = 2
            };
            _circuitBreaker = new CircuitBreaker(_config);
            _circuitBreaker.OpenedCircuitBreaker += (sender, args) => _openedEventFired = true;
            _circuitBreaker.ToleratedOpenCircuitBreaker += (sender, args) => _toleratedOpenedCount++;
        }

        protected override void When()
        {
            CallExecuteAsync();
            CallExecuteAsync();
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
            Assert.That(_caughtExceptions.Count, Is.EqualTo(4));
        }

        [Then]
        public void the_first_three_exceptions_should_be_execution_errors()
        {
            Assert.That(_caughtExceptions[0], Is.TypeOf<CircuitBreakerExecutionErrorException>());
            Assert.That(_caughtExceptions[1], Is.TypeOf<CircuitBreakerExecutionErrorException>());
            Assert.That(_caughtExceptions[2], Is.TypeOf<CircuitBreakerExecutionErrorException>());
        }

        [Then]
        public void the_fourth_exception_should_be_breaker_open()
        {
            Assert.That(_caughtExceptions[3], Is.TypeOf<CircuitBreakerOpenException>());
        }

        [Then]
        public void an_open_event_should_be_raised()
        {
            Assert.That(_openedEventFired, Is.True);
        }

        [Then]
        public void two_tolerated_open_events_should_be_raised()
        {
            Assert.That(_toleratedOpenedCount, Is.EqualTo(2));
        }
    }
}
