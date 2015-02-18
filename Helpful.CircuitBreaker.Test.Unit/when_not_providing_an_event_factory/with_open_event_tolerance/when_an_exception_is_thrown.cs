﻿using System;
using Helpful.BDD;
using Helpful.CircuitBreaker;
using Helpful.CircuitBreaker.Config;
using NUnit.Framework;

namespace when_not_providing_an_event_factory.with_open_event_tolerance
{
    class when_an_exception_is_thrown : TestBase
    {
        private CircuitBreakerConfig _config;
        private CircuitBreaker _circuitBreaker;
        private Exception _caughtException;
        private NullReferenceException _thrownException;
        private bool _breakerOpenEventTolerated;

        protected override void Given()
        {
            base.Given();
            _config = new CircuitBreakerConfig
            {
                OpenEventTolerance = 3
            };
            _circuitBreaker = new CircuitBreaker(_config);
            _circuitBreaker.ToleratedOpenCircuitBreaker += (sender, args) => _breakerOpenEventTolerated = true;
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
        public void the_breaker_raises_a_tolerated_open_event()
        {
            Assert.That(_breakerOpenEventTolerated);
        }
    }
}
