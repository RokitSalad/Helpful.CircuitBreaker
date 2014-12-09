using System;
using System.Collections.Generic;
using Helpful.BDD;
using Helpful.CircuitBreaker;
using Helpful.CircuitBreaker.Config;
using Helpful.CircuitBreaker.Events;
using Helpful.CircuitBreaker.Exceptions;
using Helpful.CircuitBreaker.Test.Unit;
using Moq;
using NUnit.Framework;

namespace when_executing_code_via_the_breaker.when_tolerating_open_events
{
    class when_receiving_an_unhandled_exceptions_within_tolerance : using_a_mocked_event_factory
    {
        private CircuitBreakerConfig _config;
        private CircuitBreaker _circuitBreaker;
        private List<Exception> _caughtExceptions;
        private ArgumentNullException _thrownException;

        protected override void Given()
        {
            base.Given();
            _caughtExceptions = new List<Exception>();
            _thrownException = new ArgumentNullException();
            _config = new CircuitBreakerConfig
            {
                OpenEventTolerance = 2,
                SchedulerConfig = new FixedRetrySchedulerConfig {RetryPeriodInSeconds = 10}
            };
            _circuitBreaker = Factory.RegisterBreaker(_config);
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
        public void two_Tolerated_open_events_should_be_raised()
        {
            ToleratedOpenEvent.Verify(e => e.RaiseEvent(It.IsAny<short>(), _config, BreakerOpenReason.Exception, It.IsAny<Exception>()), Times.Exactly(2));
        }
    }
}
