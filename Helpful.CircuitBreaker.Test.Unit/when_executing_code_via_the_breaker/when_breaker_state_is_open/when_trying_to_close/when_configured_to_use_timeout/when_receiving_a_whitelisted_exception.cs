using System;
using System.Collections.Generic;
using Helpful.BDD;
using Helpful.CircuitBreaker;
using Helpful.CircuitBreaker.Config;
using Helpful.CircuitBreaker.Test.Unit;
using Moq;
using NUnit.Framework;

namespace when_executing_code_via_the_breaker.when_breaker_state_is_open.when_trying_to_close.when_configured_to_use_timeout
{
    class when_receiving_a_whitelisted_exception : using_a_mocked_event_factory
    {
        private CircuitBreakerConfig _config;
        private CircuitBreaker _circuitBreaker;
        private Mock<IRetryScheduler> _scheduler;
        private Exception _caughtException;
        private Exception _thrownException;

        protected override void Given()
        {
            base.Given();
            _config = new CircuitBreakerConfig
            {
                ExpectedExceptionListType = ExceptionListType.WhiteList,
                ExpectedExceptionList = new List<Type> { typeof (IndexOutOfRangeException) },
                OpenEventTolerance = 5,
                UseTimeout = true,
                Timeout = TimeSpan.FromSeconds(10),
            };
            _scheduler = new Mock<IRetryScheduler>();
            _scheduler.Setup(s => s.AllowRetry).Returns(true);
            CircuitBreaker.SchedulerActivator = c => _scheduler.Object;

            _circuitBreaker = Factory.RegisterBreaker(_config);
            _thrownException = new IndexOutOfRangeException();

            _circuitBreaker.State = BreakerState.Open;

            // need to reset expectations after the constructor has run
            _scheduler.ResetCalls();
            ClosedEvent.ResetCalls();
        }

        protected override void When()
        {
            try
            {
                _circuitBreaker.Execute(() => { throw _thrownException; });
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
            ClosedEvent.Verify(e => e.RaiseEvent(_config), Times.Once);
        }

        [Then]
        public void an_exception_should_be_thrown()
        {
            Assert.That(_caughtException, Is.Not.Null);
        }

        [Then]
        public void the_exception_should_be_the_thrown_exception()
        {
            Assert.That(_caughtException, Is.InstanceOf(typeof(IndexOutOfRangeException)));
        }

        [Then]
        public void the_retry_scheduler_should_be_reset()
        {
            _scheduler.Verify(s => s.Reset(), Times.Once);
        }
    }
}
