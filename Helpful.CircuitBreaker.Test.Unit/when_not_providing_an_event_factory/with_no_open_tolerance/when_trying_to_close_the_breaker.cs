using System;
using Helpful.BDD;
using Helpful.CircuitBreaker.Config;
using NUnit.Framework;

namespace Helpful.CircuitBreaker.Test.Unit.when_not_providing_an_event_factory.with_no_open_tolerance
{
    class when_trying_to_close_the_breaker : TestBase
    {
        private CircuitBreakerConfig _config;
        private CircuitBreaker _circuitBreaker;
        private Exception _caughtException;
        private bool _breakerTryingToClose;

        protected override void Given()
        {
            base.Given();
            _config = new CircuitBreakerConfig
            {
                SchedulerConfig = new FixedRetrySchedulerConfig { RetryPeriodInSeconds = 0 }
            };
            _circuitBreaker = new CircuitBreaker(_config);
            _circuitBreaker.TryingToCloseCircuitBreaker += (sender, args) => _breakerTryingToClose = true;
            _circuitBreaker.State = BreakerState.Open;
        }

        protected override void When()
        {
            try
            {
                _circuitBreaker.Execute(() => {  });
            }
            catch (Exception e)
            {
                _caughtException = e;
            }
            finally
            {
                _circuitBreaker.Dispose();
            }
        }

        [Then]
        public void the_breaker_raises_a_tried_to_close_event()
        {
            Assert.That(_breakerTryingToClose);
        }
    }
}
