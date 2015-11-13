using Helpful.BDD;

using System;
using System.Collections.Generic;
using System.Threading;

using Helpful.CircuitBreaker.Config;
using Helpful.CircuitBreaker.Exceptions;

using NUnit.Framework;

namespace Helpful.CircuitBreaker.Test.Unit.when_executing_code_via_the_breaker.when_tolerating_open_events
{
    class when_receiving_unhandled_exceptions_beyond_tolerance_a_previous_batch_was_reset : TestBase
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
            CallExecute();
            CallExecute();
            Thread.Sleep(5000);
            CallExecute();
            CallExecute();
            CallExecute();
            CallExecute();
        }

        private void CallExecute()
        {
            try
            {
                _circuitBreaker.Execute(() => { throw _thrownException; });
            }
            catch (Exception e)
            {
                _caughtExceptions.Add(e);
            }
        }

        [Then]
        public void an_exception_should_be_thrown_for_each_call()
        {
            Assert.That(_caughtExceptions.Count, Is.EqualTo(6));
        }

        [Then]
        public void the_first_five_exceptions_should_be_execution_errors()
        {
            Assert.That(_caughtExceptions[0], Is.TypeOf<CircuitBreakerExecutionErrorException>());
            Assert.That(_caughtExceptions[1], Is.TypeOf<CircuitBreakerExecutionErrorException>());
            Assert.That(_caughtExceptions[2], Is.TypeOf<CircuitBreakerExecutionErrorException>());
            Assert.That(_caughtExceptions[3], Is.TypeOf<CircuitBreakerExecutionErrorException>());
            Assert.That(_caughtExceptions[4], Is.TypeOf<CircuitBreakerExecutionErrorException>());
        }

        [Then]
        public void the_last_exception_should_be_breaker_open()
        {
            Assert.That(_caughtExceptions[5], Is.TypeOf<CircuitBreakerOpenException>());
        }

        [Then]
        public void an_open_event_should_be_raised()
        {
            Assert.That(_openedEventFired, Is.True);
        }

        [Then]
        public void four_tolerated_open_events_should_be_raised()
        {
            Assert.That(_toleratedOpenEventCount, Is.EqualTo(4));
        }
    }
}
