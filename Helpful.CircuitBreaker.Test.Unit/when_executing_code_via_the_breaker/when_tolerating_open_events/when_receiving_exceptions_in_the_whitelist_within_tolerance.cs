﻿using System;
using System.Collections.Generic;
using Helpful.BDD;
using Helpful.CircuitBreaker;
using Helpful.CircuitBreaker.Events;
using Helpful.CircuitBreaker.Exceptions;
using Helpful.CircuitBreaker.Schedulers;
using Helpful.CircuitBreaker.Test.Unit;
using Moq;
using NUnit.Framework;

namespace when_executing_code_via_the_breaker.when_tollerating_open_events
{
    class when_receiving_exceptions_in_the_whitelist_within_tollerance : using_a_mocked_event_factory
    {
        private CircuitBreakerConfig _config;
        private CircuitBreaker _circuitBreaker;
        private IRetryScheduler _scheduler;
        private List<Exception> _caughtExceptions;
        private ArgumentNullException _thrownException;

        protected override void Given()
        {
            base.Given();
            _caughtExceptions = new List<Exception>();
            _config = new CircuitBreakerConfig { ExpectedExceptionListType = ExceptionListType.WhiteList };
            _config.ExpectedExceptionList.Add(typeof(ArgumentNullException));
            _scheduler = new FixedRetryScheduler(10);
            _circuitBreaker = Factory.GetBreaker(_config, _scheduler);
            _thrownException = new ArgumentNullException();
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
        public void no_tollerated_open_events_should_be_raised()
        {
            TolleratedOpenEvent.Verify(e => e.RaiseEvent(It.IsAny<short>(), _config, BreakerOpenReason.Exception, It.IsAny<Exception>()), Times.Never);
        }
    }
}
