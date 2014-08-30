using Helpful.BDD;
using Helpful.CircuitBreaker.Events;
using Moq;

namespace Helpful.CircuitBreaker.Test.Unit
{
    abstract class using_a_mocked_event_factory : TestBase
    {
        protected Mock<IEventFactory> EventFactory { get; private set; }
        protected CircuitBreakerFactory Factory { get; private set; }
        protected Mock<IClosedEvent> ClosedEvent { get; private set; }
        protected Mock<IOpenedEvent> OpenedEvent { get; private set; }
        protected Mock<IRegisterBreakerEvent> RegisterBreakerEvent { get; private set; }
        protected Mock<ITolleratedOpenEvent> TolleratedOpenEvent { get; private set; }
        protected Mock<ITryingToCloseEvent> TryingToCloseEvent { get; private set; }
        protected Mock<IUnregisterBreakerEvent> UnregisterBreakerEvent { get; private set; }

        protected override void Given()
        {
            EventFactory = new Mock<IEventFactory>();
            ClosedEvent = new Mock<IClosedEvent>();
            OpenedEvent = new Mock<IOpenedEvent>();
            RegisterBreakerEvent = new Mock<IRegisterBreakerEvent>();
            TolleratedOpenEvent = new Mock<ITolleratedOpenEvent>();
            TryingToCloseEvent = new Mock<ITryingToCloseEvent>();
            UnregisterBreakerEvent = new Mock<IUnregisterBreakerEvent>();
            EventFactory.Setup(ef => ef.GetClosedEvent()).Returns(ClosedEvent.Object);
            EventFactory.Setup(ef => ef.GetOpenedEvent()).Returns(OpenedEvent.Object);
            EventFactory.Setup(ef => ef.GetRegisterBreakerEvent()).Returns(RegisterBreakerEvent.Object);
            EventFactory.Setup(ef => ef.GetTolleratedOpenEvent()).Returns(TolleratedOpenEvent.Object);
            EventFactory.Setup(ef => ef.GetTriedToCloseEvent()).Returns(TryingToCloseEvent.Object);
            EventFactory.Setup(ef => ef.GetUnregisterBreakerEvent()).Returns(UnregisterBreakerEvent.Object);
            Factory = new CircuitBreakerFactory(EventFactory.Object);
        }
    }
}
