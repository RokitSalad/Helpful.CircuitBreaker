using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Helpful.BDD;
using Helpful.CircuitBreaker.Exceptions;
using NUnit.Framework;

namespace Helpful.CircuitBreaker.Test.Unit.when_executing_code_via_the_breaker
{
    class when_receiving_an_exception_before_hitting_a_timeout : using_a_mocked_event_factory
    {
        private CircuitBreakerConfig _config;
        private CircuitBreaker _circuitBreaker;
        private Exception _caughtException;
        private ArgumentNullException _thrownException;

        protected override void Given()
        {
            base.Given();
            _thrownException = new ArgumentNullException();
            _config = new CircuitBreakerConfig
            {
                Timeout = TimeSpan.FromMilliseconds(1000),
                UseTimeout = true
            };
            _circuitBreaker = Factory.GetBreaker(_config);
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
        public void the_exception_should_be_a_circuit_breaker_exception()
        {
            Assert.That(_caughtException, Is.AssignableTo<CircuitBreakerException>());
        }

        [Then]
        public void the_inner_exception_should_be_the_received_exception()
        {
            Assert.That(_caughtException.InnerException, Is.EqualTo(_thrownException));
        }
    }
}
