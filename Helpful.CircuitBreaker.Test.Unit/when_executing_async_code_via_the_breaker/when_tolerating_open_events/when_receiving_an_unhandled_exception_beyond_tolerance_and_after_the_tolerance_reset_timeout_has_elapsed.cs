using Helpful.BDD;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Helpful.CircuitBreaker.Config;
using Helpful.CircuitBreaker.Exceptions;

using NUnit.Framework;

namespace Helpful.CircuitBreaker.Test.Unit.when_executing_async_code_via_the_breaker.when_tolerating_open_events
{
    class when_receiving_an_unhandled_exception_beyond_tolerance_and_after_the_tolerance_reset_timeout_has_elapsed : TestBase
    {
        private CircuitBreakerConfig _config;
        private CircuitBreaker _circuitBreaker;
        private List<Exception> _caughtExceptions;
        private ArgumentNullException _thrownException;
        private int _toleratedOpenEventCount;
        private bool _openedEventFired;

        protected override void Given()
        {
            base.Given();
            _caughtExceptions = new List<Exception>();
            _thrownException = new ArgumentNullException();
            _config = new CircuitBreakerConfig
            {
                OpenEventTolerance = 2,
                OpenEventToleranceResetPeriod = new TimeSpan(0, 0, 3)
            };
            _circuitBreaker = new CircuitBreaker(_config);
            _circuitBreaker.ToleratedOpenCircuitBreaker += (sender, args) => _toleratedOpenEventCount++;
            _circuitBreaker.OpenedCircuitBreaker += (sender, args) => _openedEventFired = true;
        }

        protected override void When()
        {
            CallExecuteAsync();
            CallExecuteAsync();
            Thread.Sleep(5000);
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
            Assert.That(_caughtExceptions.Count, Is.EqualTo(3));
        }

        [Then]
        public void all_three_exceptions_should_be_execution_errors()
        {
            Assert.That(_caughtExceptions[0], Is.TypeOf<CircuitBreakerExecutionErrorException>());
            Assert.That(_caughtExceptions[1], Is.TypeOf<CircuitBreakerExecutionErrorException>());
            Assert.That(_caughtExceptions[2], Is.TypeOf<CircuitBreakerExecutionErrorException>());
        }

        [Then]
        public void an_open_event_should_not_be_raised()
        {
            Assert.That(_openedEventFired, Is.False);
        }

        [Then]
        public void three_tolerated_open_events_should_be_raised()
        {
            Assert.That(_toleratedOpenEventCount, Is.EqualTo(3));
        }
    }
}
