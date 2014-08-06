using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Helpful.CircuitBreaker
{
    public class CircuitBreakerFactory : IDisposable
    {
        private readonly IEventFactory _eventFactory;
        private readonly ICollection<CircuitBreaker> _breakers;
        private readonly IUnregisterBreakerEvent _unregisterBreakerEvent;
        private readonly IRegisterBreakerEvent _registerBreakerEvent;
        private bool _disposed;

        public CircuitBreakerFactory(IEventFactory eventFactory)
        {
            _eventFactory = eventFactory;
            _registerBreakerEvent = eventFactory.GetRegisterBreakerEvent();
            _unregisterBreakerEvent = eventFactory.GetUnregisterBreakerEvent();
            _breakers = new Collection<CircuitBreaker>();
        }

        public CircuitBreaker GetBreaker()
        {
            CircuitBreaker breaker = new CircuitBreaker(_eventFactory.GetClosedEvent(), _eventFactory.GetOpenedEvent(),
                _eventFactory.GetTriedToCloseEvent());
            _breakers.Add(breaker);
            _registerBreakerEvent.RaiseEvent(breaker);
            return breaker;
        }

        ~CircuitBreakerFactory()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                foreach (CircuitBreaker breaker in _breakers)
                {
                    _unregisterBreakerEvent.RaiseEvent(breaker);
                }
                _breakers.Clear();
            }
        }
    }
}
