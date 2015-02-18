using System;
using System.Collections.Generic;
using System.Threading;
using Helpful.BDD;
using Helpful.CircuitBreaker;
using Helpful.CircuitBreaker.Config;
using Helpful.CircuitBreaker.Exceptions;
using NUnit.Framework;

namespace when_executing_code_via_the_breaker.when_tolerating_open_events
{
    class when_hitting_timeouts_beyond_tolerance : TestBase
    {
        private CircuitBreakerConfig _config;
        private CircuitBreaker _circuitBreaker;
        private List<Exception> _caughtExceptions;
        private bool _openedEventFired;
        private int _toleratedOpenedCount;

        protected override void Given()
        {
            base.Given();
            _caughtExceptions = new List<Exception>();
            _config = new CircuitBreakerConfig
            {
                UseTimeout = true,
                Timeout = TimeSpan.FromMilliseconds(0),
                OpenEventTolerance = 2
            };

            _circuitBreaker = new CircuitBreaker(_config);
            _circuitBreaker.OpenedCircuitBreaker += (sender, args) => _openedEventFired = true;
            _circuitBreaker.ToleratedOpenCircuitBreaker += (sender, args) => _toleratedOpenedCount++;
        }

        protected override void When()
        {
            CallExecute();
            CallExecute();
            CallExecute();
            CallExecute();
        }

        private void CallExecute()
        {
            try
            {
                _circuitBreaker.Execute(() => Thread.Sleep(10000));
            }
            catch (Exception e)
            {
                _caughtExceptions.Add(e);
            }
        }

        [Then]
        public void each_call_should_throw_an_exception()
        {
            Assert.That(_caughtExceptions.Count, Is.EqualTo(4));
        }

        [Then]
        public void the_first_three_exceptions_should_be_timeouts()
        {
            Assert.That(_caughtExceptions[0], Is.TypeOf<CircuitBreakerTimedOutException>());
            Assert.That(_caughtExceptions[1], Is.TypeOf<CircuitBreakerTimedOutException>());
            Assert.That(_caughtExceptions[2], Is.TypeOf<CircuitBreakerTimedOutException>());
        }

        [Then]
        public void the_fourth_exception_should_be_breaker_open()
        {
            Assert.That(_caughtExceptions[3], Is.TypeOf<CircuitBreakerOpenException>());
        }

        [Then]
        public void the_breaker_should_be_open()
        {
            Assert.That(_circuitBreaker.State, Is.EqualTo(BreakerState.Open));
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
