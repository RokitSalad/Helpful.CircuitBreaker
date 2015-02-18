using System;
using System.Threading;
using Helpful.BDD;
using Helpful.CircuitBreaker;
using Helpful.CircuitBreaker.Config;
using Helpful.CircuitBreaker.Exceptions;
using NUnit.Framework;

namespace when_scheduling_retries.when_there_is_a_single_open_period
{
    class with_a_single_error : TestBase
    {
        private CircuitBreakerConfig _config;
        private CircuitBreaker _circuitBreaker;
        private Exception _caughtException;
        private Exception _testException;
        private int _breakerOpenedCount;
        private int _breakerTryingToCloseCount;
        private int _breakerClosedCount;

        protected override void Given()
        {
            base.Given();
            _config = new CircuitBreakerConfig
            {
                BreakerOpenPeriods = new[] {TimeSpan.FromSeconds(1)}
            };
            _circuitBreaker = new CircuitBreaker(_config);
            _circuitBreaker.OpenedCircuitBreaker += (sender, args) => _breakerOpenedCount++;
            _circuitBreaker.TryingToCloseCircuitBreaker += (sender, args) => _breakerTryingToCloseCount++;
            _circuitBreaker.ClosedCircuitBreaker += (sender, args) => _breakerClosedCount++;
        }

        protected override void When()
        {
            try
            {
                TriggerBreakerOpen();
                _circuitBreaker.Execute(() => { });
            }
            catch (CircuitBreakerOpenException e)
            {
                _caughtException = e;
                Thread.Sleep(TimeSpan.FromSeconds(1));
                try
                {
                    _circuitBreaker.Execute(() => { });
                }
                catch (Exception ex)
                {
                    _testException = ex;
                }
            }
        }

        private void TriggerBreakerOpen()
        {
            try
            {
                _circuitBreaker.Execute(() => { throw new Exception(); });
            }
            catch (Exception)
            {
                //swallow the exception
            }
        }

        [Then]
        public void a_breaker_open_exception_should_have_been_thrown()
        {
            Assert.That(_caughtException, Is.TypeOf<CircuitBreakerOpenException>());
        }

        [Then]
        public void after_the_timeout_the_execution_should_have_succeeded()
        {
            Assert.That(_testException, Is.Null);
        }

        [Then]
        public void the_breaker_should_have_opened_once()
        {
            Assert.That(_breakerOpenedCount, Is.EqualTo(1));
        }

        [Then]
        public void the_breaker_should_have_closed_twice()
        {
            Assert.That(_breakerClosedCount, Is.EqualTo(2));
        }

        [Then]
        public void the_breaker_should_have_been_trying_to_close_once()
        {
            Assert.That(_breakerTryingToCloseCount, Is.EqualTo(1));
        }
    }
}
