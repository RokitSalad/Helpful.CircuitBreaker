using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Helpful.BDD;
using Helpful.CircuitBreaker;
using Helpful.CircuitBreaker.Config;
using Helpful.CircuitBreaker.Exceptions;
using NUnit.Framework;

namespace when_executing_async_code_via_the_breaker.when_tolerating_open_events
{
    class when_hitting_timeouts_within_tolerance : TestBase
    {
        private CircuitBreakerConfig _config;
        private TimeSpan _timeout;
        private CircuitBreaker _circuitBreaker;
        private List<Exception> _caughtExceptions;
        private int _toleratedOpenedCount;

        protected override void Given()
        {
            base.Given();
            _caughtExceptions = new List<Exception>();
            _timeout = TimeSpan.FromMilliseconds(500);
            _config = new CircuitBreakerConfig
                {
                    UseTimeout = true,
                    Timeout = _timeout,
                    OpenEventTolerance = 2
                };
            _circuitBreaker = new CircuitBreaker(_config);
            _circuitBreaker.ToleratedOpenCircuitBreaker += (sender, args) => _toleratedOpenedCount++;
        }

        protected override void When()
        {
            CallExecuteAsync();
            CallExecuteAsync();
        }

        private void CallExecuteAsync()
        {
            try
            {
                Func<Task> action = async () =>
                {
                    await Task.Delay(10000);
                };

                _circuitBreaker.ExecuteAsync(action).Wait();
            }
            catch (AggregateException ae)
            {
                _caughtExceptions.Add(ae.InnerException);
            }
            catch (Exception e)
            {
                _caughtExceptions.Add(e);
            }
        }

        [Then]
        public void each_timeout_should_generate_an_exception()
        {
            Assert.That(_caughtExceptions.Count, Is.EqualTo(2));
        }

        [Then]
        public void no_exceptions_should_be_breaker_open_exceptions()
        {
            Assert.That(_caughtExceptions, Has.No.TypeOf<CircuitBreakerOpenException>());
        }

        [Then]
        public void the_breaker_should_remain_closed()
        {
            Assert.That(_circuitBreaker.State, Is.EqualTo(BreakerState.Closed));
        }

        [Then]
        public void two_tolerated_open_events_should_be_raised()
        {
            Assert.That(_toleratedOpenedCount, Is.EqualTo(2));
        }
    }
}
