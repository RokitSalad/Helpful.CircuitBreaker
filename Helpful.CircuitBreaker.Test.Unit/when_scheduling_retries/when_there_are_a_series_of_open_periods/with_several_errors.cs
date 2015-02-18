using System;
using System.Threading;
using Helpful.BDD;
using Helpful.CircuitBreaker;
using Helpful.CircuitBreaker.Config;
using Helpful.CircuitBreaker.Exceptions;
using NUnit.Framework;

namespace when_scheduling_retries.when_there_are_a_series_of_open_periods
{
    class with_several_errors : TestBase
    {
        private CircuitBreakerConfig _config;
        private CircuitBreaker _circuitBreaker;
        private Exception _caughtException;
        private int _breakerOpenedCount;
        private int _breakerTryingToCloseCount;
        private int _breakerClosedCount;

        protected override void Given()
        {
            base.Given();
            _config = new CircuitBreakerConfig
            {
                BreakerOpenPeriods = new[] { TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1) }
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
                Thread.Sleep(TimeSpan.FromMilliseconds(3100));
                TriggerBreakerOpen();
                Thread.Sleep(TimeSpan.FromMilliseconds(2100));
                TriggerBreakerOpen();
                Thread.Sleep(TimeSpan.FromMilliseconds(1100));
                TriggerBreakerOpen();
            }
            catch (CircuitBreakerOpenException e)
            {
                _caughtException = e;
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
        public void the_breaker_should_have_opened_three_times()
        {
            Assert.That(_breakerOpenedCount, Is.EqualTo(4));
        }

        [Then]
        public void the_breaker_should_have_tried_to_close_twice()
        {
            Assert.That(_breakerTryingToCloseCount, Is.EqualTo(3));
        }

        [Then]
        public void the_breaker_should_have_closed_twice()
        {
            Assert.That(_breakerTryingToCloseCount, Is.EqualTo(3));
        }
    }
}
