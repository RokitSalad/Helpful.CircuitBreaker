using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helpful.BDD;
using Helpful.CircuitBreaker;
using Helpful.CircuitBreaker.Events;
using Moq;

namespace when_requesting_a_breaker
{
    class when_setting_a_timeout : TestBase
    {
        private Mock<IEventFactory> _eventFactory;
        private CircuitBreakerFactory _factory;

        protected override void Given()
        {
            _eventFactory = new Mock<IEventFactory>();
            _factory = new CircuitBreakerFactory(_eventFactory.Object);
        }

        protected override void When()
        {
            
        }

        [Then]
        public void the_timeout_is_set()
        {
            
        }
    }
}
