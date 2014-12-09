using Helpful.BDD;
using Helpful.CircuitBreaker;
using Helpful.CircuitBreaker.Config;
using Helpful.CircuitBreaker.Test.Unit;
using NUnit.Framework;

namespace when_requesting_a_breaker
{
    class when_setting_configuration : using_a_mocked_event_factory
    {
        private CircuitBreaker _circuitBreaker;
        private CircuitBreakerConfig _config;

        protected override void Given()
        {
            base.Given();
            _config = new CircuitBreakerConfig
            {
                BreakerId = "MyTestBreaker",
                SchedulerConfig = new FixedRetrySchedulerConfig {RetryPeriodInSeconds = 10}
            };
        }

        protected override void When()
        {
            Factory.RegisterBreaker(_config);
            _circuitBreaker = Factory.GetBreaker("MyTestBreaker");
        }

        [Then]
        public void config_is_injected_into_the_breaker()
        {
            Assert.That(_circuitBreaker.Config, Is.EqualTo(_config)); 
        }
    }
}
