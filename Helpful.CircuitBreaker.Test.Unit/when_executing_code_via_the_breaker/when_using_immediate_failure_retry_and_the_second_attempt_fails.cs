using Helpful.BDD;

using System;
using System.Collections.Generic;

using Helpful.CircuitBreaker.Config;

using NUnit.Framework;

namespace Helpful.CircuitBreaker.Test.Unit.when_executing_code_via_the_breaker
{
    class when_using_immediate_failure_retry_and_the_second_attempt_fails : TestBase
    {
        private CircuitBreakerConfig _config;
        private CircuitBreaker _circuitBreaker;
        private List<Exception> _caughtExceptions;
        private bool _openedEventFired;

        protected override void Given()
        {
            base.Given();
            _caughtExceptions = new List<Exception>();
            _config = new CircuitBreakerConfig
            {
                UseImmediateFailureRetry = true
            };

            _circuitBreaker = new CircuitBreaker(_config);
            _circuitBreaker.OpenedCircuitBreaker += (sender, args) => _openedEventFired = true;
        }

        protected override void When()
        {
            CallExecute();
        }

        private void CallExecute()
        {
            try
            {
                _circuitBreaker.Execute(() => ActionResult.Failure);
            }
            catch (Exception e)
            {
                _caughtExceptions.Add(e);
            }
        }

        [Then]
        public void the_call_should_throw_an_exception()
        {
            Assert.That(_caughtExceptions.Count, Is.EqualTo(1));
        }

        [Then]
        public void the_breaker_should_be_open()
        {
            Assert.That(_circuitBreaker.State, Is.EqualTo(BreakerState.Open));
        }

        [Then]
        public void an_open_event_should_be_raised()
        {
            Assert.That(_openedEventFired, Is.True);
        }
    }
}
