namespace when_executing_code_via_the_breaker
{
    using System;
    using System.Threading;
    using Helpful.BDD;
    using Helpful.CircuitBreaker;
    using Helpful.CircuitBreaker.Config;
    using Helpful.CircuitBreaker.Exceptions;
    using Helpful.CircuitBreaker.Test.Unit;
    using NUnit.Framework;

    internal class when_hitting_a_timeout : using_a_mocked_event_factory
    {
        private CircuitBreakerConfig _config;
        private TimeSpan _timeout;
        private CircuitBreaker _circuitBreaker;
        private Exception _caughtException;

        protected override void Given()
        {
            base.Given();
            _timeout = TimeSpan.FromMilliseconds(1000);
            _config = new CircuitBreakerConfig
            {
                UseTimeout = true,
                Timeout = _timeout,
                SchedulerConfig = new FixedRetrySchedulerConfig {RetryPeriodInSeconds = 10}
            };
            _circuitBreaker = Factory.RegisterBreaker(_config);
        }

        protected override void When()
        {
            try
            {
                _circuitBreaker.Execute(() => Thread.Sleep(10000));
            }
            catch (Exception e)
            {
                _caughtException = e;
            }
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
        public void the_breaker_should_open()
        {
            Assert.That(_circuitBreaker.State, Is.EqualTo(BreakerState.Open));
        }
    }
}