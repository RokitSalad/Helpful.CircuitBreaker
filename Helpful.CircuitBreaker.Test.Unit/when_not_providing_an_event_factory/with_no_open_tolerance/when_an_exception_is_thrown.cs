using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Helpful.BDD;
using Helpful.CircuitBreaker;
using Helpful.CircuitBreaker.Config;
using NUnit.Framework;

namespace when_not_providing_an_event_factory.with_no_open_tolerance
{
    class when_an_exception_is_thrown : TestBase
    {
        private CircuitBreakerConfig _config;
        private CircuitBreaker _circuitBreaker;
        private Exception _caughtException;
        private NullReferenceException _thrownException;
        private bool _breakerRegistered;
        private bool _breakerClosed;
        private bool _breakerOpened;
        private bool _breakerUnregistered;

        protected override void Given()
        {
            base.Given();
            _config = new CircuitBreakerConfig
            {
                SchedulerConfig = new FixedRetrySchedulerConfig {RetryPeriodInSeconds = 10}
            };
            _circuitBreaker = new CircuitBreaker(_config);
            _circuitBreaker.RegisterCircuitBreaker += (sender, args) => _breakerRegistered = true;
            _circuitBreaker.ClosedCircuitBreaker += (sender, args) => _breakerClosed = true;
            _circuitBreaker.OpenedCircuitBreaker += (sender, args) => _breakerOpened = true;
            _circuitBreaker.UnregisterCircuitBreaker += (sender, args) => _breakerUnregistered = true;
            _thrownException = new NullReferenceException();
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
            finally
            {
                _circuitBreaker.Dispose();
            }
        }

        [Then]
        public void the_breaker_raises_a_register_event()
        {
            Assert.That(_breakerRegistered);
        }

        [Then]
        public void the_breaker_raises_a_closed_event()
        {
            Assert.That(_breakerClosed);
        }

        [Then]
        public void the_breaker_raises_an_open_event()
        {
            Assert.That(_breakerOpened);
        }

        [Then]
        public void the_breaker_raises_an_unregister_event()
        {
            Assert.That(_breakerUnregistered);
        }
    }
}
