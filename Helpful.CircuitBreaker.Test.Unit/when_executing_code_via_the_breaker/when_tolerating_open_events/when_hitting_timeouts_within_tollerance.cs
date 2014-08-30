using System;
using System.Collections.Generic;
using System.Threading;
using Helpful.BDD;
using Helpful.CircuitBreaker;
using Helpful.CircuitBreaker.Exceptions;
using Helpful.CircuitBreaker.Test.Unit;
using Moq;
using NUnit.Framework;

namespace when_executing_code_via_the_breaker.when_tolerating_open_events
{
    class when_hitting_timeouts_within_tollerance : using_a_mocked_event_factory
    {
        private CircuitBreakerConfig _config;
        private TimeSpan _timeout;
        private CircuitBreaker _circuitBreaker;
        private List<Exception> _caughtExceptions;

        protected override void Given()
        {
            base.Given();
            _caughtExceptions = new List<Exception>();
            _timeout = TimeSpan.FromMilliseconds(500);
            _config = new CircuitBreakerConfig
                {
                    UseTimeout = true,
                    Timeout = _timeout,
                    OpenEventTolerance = 2
                };
            _circuitBreaker = Factory.GetBreaker(_config);
        }

        protected override void When()
        {
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
        public void each_timeout_should_generate_an_exception()
        {
            Assert.That(_caughtExceptions.Count, Is.EqualTo(2));
        }

        [Then]
        public void no_exceptions_should_be_breaker_open_exceptions()
        {
            Assert.That(_caughtExceptions, Has.No.TypeOf<CircuitBreakerOpenException>());
        }

        [Then]
        public void the_breaker_should_remain_closed()
        {
            Assert.That(_circuitBreaker.State, Is.EqualTo(BreakerState.Closed));
        }

        [Then]
        public void two_tollerated_open_events_should_be_raised()
        {
            TolleratedOpenEvent.Verify(e => e.RaiseEvent(It.IsAny<short>(), _config, BreakerOpenReason.Timeout, It.IsAny<Exception>()), Times.Exactly(2));
        }
    }
}
