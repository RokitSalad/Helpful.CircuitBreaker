using System;
using Helpful.BDD;
using Helpful.CircuitBreaker;
using Helpful.CircuitBreaker.Events;
using Moq;
using NUnit.Framework;

namespace when_requesting_a_breaker
{
    class when_setting_configuration : TestBase
    {
        private Mock<IEventFactory> _eventFactory;
        private CircuitBreakerFactory _factory;
        private CircuitBreaker _circuitBreaker;
        private TimeSpan _timeout;
        private CircuitBreakerConfig _config;

        protected override void Given()
        {
            _eventFactory = new Mock<IEventFactory>();
            _eventFactory.Setup(ef => ef.GetClosedEvent()).Returns(new Mock<IClosedEvent>().Object);
            _eventFactory.Setup(ef => ef.GetOpenedEvent()).Returns(new Mock<IOpenedEvent>().Object);
            _eventFactory.Setup(ef => ef.GetRegisterBreakerEvent()).Returns(new Mock<IRegisterBreakerEvent>().Object);
            _eventFactory.Setup(ef => ef.GetTolleratedOpenEvent()).Returns(new Mock<ITolleratedOpenEvent>().Object);
            _eventFactory.Setup(ef => ef.GetTriedToCloseEvent()).Returns(new Mock<ITriedToCloseEvent>().Object);
            _eventFactory.Setup(ef => ef.GetUnregisterBreakerEvent()).Returns(new Mock<IUnregisterBreakerEvent>().Object);
            _factory = new CircuitBreakerFactory(_eventFactory.Object);
            _timeout = TimeSpan.FromMilliseconds(1000);
            _config = new CircuitBreakerConfig
                {
                    Timeout = _timeout
                };
        }

        protected override void When()
        {
            _circuitBreaker = _factory.GetBreaker(_config);
        }

        [Then]
        public void config_is_injected_into_the_breaker()
        {
            Assert.That(_circuitBreaker.Config, Is.EqualTo(_config)); 
        }
    }
}
