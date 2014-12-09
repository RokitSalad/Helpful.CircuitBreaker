namespace when_executing_code_via_the_breaker.when_breaker_state_is_open.when_trying_to_close.when_configured_to_use_timeout
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Helpful.BDD;
    using Helpful.CircuitBreaker;
    using Helpful.CircuitBreaker.Config;
    using Helpful.CircuitBreaker.Events;
    using Helpful.CircuitBreaker.Exceptions;
    using Helpful.CircuitBreaker.Test.Unit;
    using Moq;
    using NUnit.Framework;

    class when_hitting_a_timeout : using_a_mocked_event_factory
    {
        private CircuitBreakerConfig _config;
        private CircuitBreaker _circuitBreaker;
        private Mock<IRetryScheduler> _scheduler;
        private Exception _caughtException;



        protected override void Given()
        {
            base.Given();
            _config = new CircuitBreakerConfig
            {
                ExpectedExceptionListType = ExceptionListType.BlackList,
                ExpectedExceptionList = new List<Type> { typeof (IndexOutOfRangeException) },
                OpenEventTolerance = 5,
                UseTimeout = true,
                Timeout = TimeSpan.FromMilliseconds(100),
                SchedulerConfig = new FixedRetrySchedulerConfig { RetryPeriodInSeconds = 10}
            };
            _scheduler = new Mock<IRetryScheduler>();
            CircuitBreaker.SchedulerActivator = config => _scheduler.Object;

            _scheduler.Setup(s => s.AllowRetry).Returns(true);
            _circuitBreaker = Factory.RegisterBreaker(_config);
            _circuitBreaker.State = BreakerState.Open;
        }

        protected override void When()
        {
            try
            {
                _circuitBreaker.Execute(() => Thread.Sleep(1000));
            }
            catch (Exception e)
            {
                _caughtException = e;
            }
        }

        [Then]
        public void the_breaker_should_reopen()
        {
            Assert.That(_circuitBreaker.State, Is.EqualTo(BreakerState.Open));
        }

        [Then]
        public void the_open_event_should_be_fired()
        {
            OpenedEvent.Verify(e => e.RaiseEvent(_config, BreakerOpenReason.Timeout, null), Times.Once);
        }
      
        [Then]
        public void no_exceptions_should_be_Tolerated()
        {
            ToleratedOpenEvent.Verify(e => e.RaiseEvent(It.IsAny<short>(), _config, 
                It.IsAny<BreakerOpenReason>(), It.IsAny<Exception>()), Times.Never);
        }

        [Then]
        public void an_exception_should_be_thrown()
        {
            Assert.That(_caughtException, Is.Not.Null);
        }

        [Then]
        public void the_exception_should_be_a_circuit_breaker_exception()
        {
            Assert.That(_caughtException, Is.AssignableTo<CircuitBreakerException>());
        }

        [Then]
        public void the_exception_should_be_specifically_a_timeout_exception()
        {
            Assert.That(_caughtException, Is.AssignableTo<CircuitBreakerTimedOutException>());
        }
 
        [Then]
        public void the_retry_scheduler_should_begin_again()
        {
            _scheduler.Verify(s => s.BeginNextPeriod(It.IsAny<DateTime>()), Times.Once);
        }
    }
}
