namespace Helpful.CircuitBreaker
{
    using System;
    using System.Collections.Concurrent;
    using Config;
    using Events;

    /// <summary>
    /// Registers and issues circuit breakers 
    /// </summary>
    public class CircuitBreakerFactory : IDisposable
    {
        private readonly ConcurrentDictionary<string, CircuitBreaker> _breakers =
            new ConcurrentDictionary<string, CircuitBreaker>();

        private readonly IEventFactory _eventFactory;
        private readonly IRegisterBreakerEvent _registerBreakerEvent;
        private readonly IUnregisterBreakerEvent _unregisterBreakerEvent;
        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CircuitBreakerFactory" /> class.
        /// </summary>
        /// <param name="eventFactory">The event factory.</param>
        /// <exception cref="ArgumentNullException">The value of 'eventFactory' cannot be null. </exception>
        /// <exception cref="InvalidOperationException">eventFactory did not return or implement all required event handlers</exception>
        public CircuitBreakerFactory(IEventFactory eventFactory)
        {
            if (eventFactory == null) 
                throw new ArgumentNullException("eventFactory");
            if(!eventFactory.Validate())
                throw new InvalidOperationException("eventFactory did not return or implement all required event handlers");

            _eventFactory = eventFactory;
            _registerBreakerEvent = eventFactory.GetRegisterBreakerEvent();
            _unregisterBreakerEvent = eventFactory.GetUnregisterBreakerEvent();
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                foreach (CircuitBreaker breaker in _breakers.Values)
                {
                    _unregisterBreakerEvent.RaiseEvent(breaker.Config);
                }
                _breakers.Clear();
            }
        }

        /// <summary>
        ///     Registers a new breaker with the specified configuration and returns the breaker
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <returns>The registered circuit breaker <see cref="CircuitBreaker" /></returns>
        /// <exception cref="ArgumentNullException">The value of 'config' cannot be null.</exception>
        /// <exception cref="OverflowException">The maximum number of breakers has been reached <see cref="F:System.Int32.MaxValue" />.</exception>
        public CircuitBreaker RegisterBreaker(CircuitBreakerConfig config)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            config.BreakerId = config.BreakerId ?? Guid.NewGuid().ToString();

            var breaker = new CircuitBreaker(
                _eventFactory.GetClosedEvent(),
                _eventFactory.GetOpenedEvent(),
                _eventFactory.GetTriedToCloseEvent(),
                _eventFactory.GetTolleratedOpenEvent(),
                config
                );

            _registerBreakerEvent.RaiseEvent(breaker.Config);

            _breakers.TryAdd(config.BreakerId, breaker);
            return breaker;
        }

        /// <summary>
        ///     Gets a previously registered breaker.
        /// </summary>
        /// <param name="breakerId">The breaker identifier.</param>
        /// <returns>The registered circuit breaker  <see cref="CircuitBreaker" /> or null if the circuit breaker has not been found </returns>
        /// <exception cref="ArgumentNullException">The value of 'breakerId' cannot be null. </exception>
        public CircuitBreaker GetBreaker(string breakerId)
        {
            if (breakerId == null)
                throw new ArgumentNullException("breakerId");

            CircuitBreaker breaker;
            _breakers.TryGetValue(breakerId, out breaker);
            return breaker;
        }

        /// <summary>
        ///     Allows an object to try to free resources and perform other cleanup operations before it is reclaimed by garbage
        ///     collection.
        /// </summary>
        ~CircuitBreakerFactory()
        {
            Dispose();
        }
    }
}