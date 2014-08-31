using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Helpful.CircuitBreaker.Events;

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

        public CircuitBreaker GetBreaker(CircuitBreakerConfig config)
        {
            CircuitBreaker breaker = new CircuitBreaker(_eventFactory.GetClosedEvent(), _eventFactory.GetOpenedEvent(),
                _eventFactory.GetTriedToCloseEvent(), _eventFactory.GetTolleratedOpenEvent(), config);
            _breakers.Add(breaker);
            _registerBreakerEvent.RaiseEvent(breaker.Config);
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
                    _unregisterBreakerEvent.RaiseEvent(breaker.Config);
                }
                _breakers.Clear();
            }
        }
    }
}
