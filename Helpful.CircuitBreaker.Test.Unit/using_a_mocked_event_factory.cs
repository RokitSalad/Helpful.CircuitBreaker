using Helpful.BDD;
using Helpful.CircuitBreaker.Events;
using Moq;

namespace Helpful.CircuitBreaker.Test.Unit
{
    abstract class using_a_mocked_event_factory : TestBase
    {
        protected Mock<IEventFactory> EventFactory { get; private set; }
        protected CircuitBreakerFactory Factory { get; private set; }

        protected override void Given()
        {
            EventFactory = new Mock<IEventFactory>();
            EventFactory.Setup(ef => ef.GetClosedEvent()).Returns(new Mock<IClosedEvent>().Object);
            EventFactory.Setup(ef => ef.GetOpenedEvent()).Returns(new Mock<IOpenedEvent>().Object);
            EventFactory.Setup(ef => ef.GetRegisterBreakerEvent()).Returns(new Mock<IRegisterBreakerEvent>().Object);
            EventFactory.Setup(ef => ef.GetTolleratedOpenEvent()).Returns(new Mock<ITolleratedOpenEvent>().Object);
            EventFactory.Setup(ef => ef.GetTriedToCloseEvent()).Returns(new Mock<ITriedToCloseEvent>().Object);
            EventFactory.Setup(ef => ef.GetUnregisterBreakerEvent()).Returns(new Mock<IUnregisterBreakerEvent>().Object);
            Factory = new CircuitBreakerFactory(EventFactory.Object);
        }
    }
}
