﻿using System;
using Helpful.BDD;
using Helpful.CircuitBreaker;
using Helpful.CircuitBreaker.Test.Unit;
using NUnit.Framework;

namespace when_executing_code_via_the_breaker
{
    class when_receiving_an_exception_in_the_whitelist : using_a_mocked_event_factory
    {
        private CircuitBreakerConfig _config;
        private CircuitBreaker _circuitBreaker;
        private Exception _caughtException;
        private ArgumentNullException _thrownException;

        protected override void Given()
        {
            base.Given();
            _config = new CircuitBreakerConfig { ExpectedExceptionListType = ExceptionListType.WhiteList };
            _config.ExpectedExceptionList.Add(typeof(ArgumentNullException));
            _circuitBreaker = Factory.GetBreaker(_config);
            _thrownException = new ArgumentNullException();
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
        public void an_exception_should_be_thrown()
        {
            Assert.That(_caughtException, Is.Not.Null);
        }

        [Then]
        public void the_exception_should_be_the_thrown_exception()
        {
            Assert.That(_caughtException, Is.EqualTo(_thrownException));
        }

        [Then]
        public void the_breaker_should_remain_closed()
        {
            Assert.That(_circuitBreaker.State, Is.EqualTo(BreakerState.Closed));
        }
    }
}
