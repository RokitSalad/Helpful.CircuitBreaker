using System;
using System.Linq;
using System.Threading.Tasks;
using Helpful.CircuitBreaker.Config;
using Helpful.CircuitBreaker.Events;
using Helpful.CircuitBreaker.Exceptions;

namespace Helpful.CircuitBreaker
{
    /// <summary>
    /// </summary>
    public class CircuitBreaker : ICircuitBreaker, IDisposable
    {
        internal static Func<ISchedulerConfig, IRetryScheduler> SchedulerActivator { get; set; }

        private readonly IClosedEvent _closedEventHandler;
        private readonly IOpenedEvent _openedEventHandler;
        private readonly ITolleratedOpenEvent _toleratedOpenEventHandler;
        private readonly ITryingToCloseEvent _tryingToCloseEventHandler;
        private readonly IUnregisterBreakerEvent _unregisterEventHandler;
        private readonly IRegisterBreakerEvent _registerEventHandler;

        private readonly bool _useOldEventing;
        private bool _disposed;
        private CircuitBreakerConfig _config;
        private IRetryScheduler _retryScheduler;

        private short _toleratedOpenEventCount;

        /// <summary>
        /// Raised when the circuit breaker enters the closed state
        /// </summary>
        public event EventHandler<CircuitBreakerEventArgs> ClosedCircuitBreaker;

        /// <summary>
        /// Raised when the circuit breaker enters the opened state
        /// </summary>
        public event EventHandler<OpenedCircuitBreakerEventArgs> OpenedCircuitBreaker;

        /// <summary>
        /// Raised when trying to close the circuit breaker
        /// </summary>
        public event EventHandler<CircuitBreakerEventArgs> TryingToCloseCircuitBreaker;

        /// <summary>
        /// Raised when the breaker tries to open but remains closed due to tolerance
        /// </summary>
        public event EventHandler<ToleratedOpenCircuitBreakerEventArgs> ToleratedOpenCircuitBreaker;

        /// <summary>
        /// Raised when the circuit breaker is disposed
        /// </summary>
        public event EventHandler<CircuitBreakerEventArgs> UnregisterCircuitBreaker;

        /// <summary>
        /// Raised when a circuit breaker is first used
        /// </summary>
        public event EventHandler<CircuitBreakerEventArgs> RegisterCircuitBreaker;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="eventFactory">The factory for generating event handlers</param>
        /// <param name="config">The config for the breaker</param>
        [Obsolete("This constructor is now obsolete. Preferred management of events is via classic event handlers. Do not inject an implementation of IEventFactory.")]
        public CircuitBreaker(IEventFactory eventFactory, CircuitBreakerConfig config)
        {
            _useOldEventing = true;
            _closedEventHandler = eventFactory.GetClosedEvent();
            _openedEventHandler = eventFactory.GetOpenedEvent();
            _tryingToCloseEventHandler = eventFactory.GetTriedToCloseEvent();
            _toleratedOpenEventHandler = eventFactory.GetTolleratedOpenEvent();
            _unregisterEventHandler = eventFactory.GetUnregisterBreakerEvent();
            _registerEventHandler = eventFactory.GetRegisterBreakerEvent();

            Initialise(config);
        }

        /// <summary>
        /// Constructor without an event factory - the breaker will just raise normal .Net events for you to handle
        /// </summary>
        /// <param name="config">The config for the breaker</param>
        public CircuitBreaker(CircuitBreakerConfig config)
        {
            _useOldEventing = false;

            Initialise(config);
        }

        private void Initialise(CircuitBreakerConfig config)
        {
            _config = config;
            _retryScheduler = SchedulerActivator == null
                ? DefaultSchedulerActivator(config.SchedulerConfig)
                : SchedulerActivator(config.SchedulerConfig);
        }

        /// <summary>
        ///     Gets the state.
        /// </summary>
        /// <value>
        ///     The state.
        /// </value>
        public BreakerState State { get; internal set; }

        /// <summary>
        /// Gets the breaker identifier.
        /// </summary>
        /// <value>
        /// The breaker identifier.
        /// </value>
        public string BreakerId
        {
            get { return _config.BreakerId; }
        }

        internal CircuitBreakerConfig Config
        {
            get { return _config; }
        }

        private IRetryScheduler DefaultSchedulerActivator(ISchedulerConfig schedulerConfig)
        {
            try
            {
                Type implementationOfIRetryScheduler = schedulerConfig.ImplementationOfIRetryScheduler;

                if (!typeof (IRetryScheduler).IsAssignableFrom(implementationOfIRetryScheduler))
                {
                    throw new ArgumentException("ImplementationOfIRetryScheduler does not implement IRetryScheduler");                    
                }
                var retryScheduler = Activator.CreateInstance(implementationOfIRetryScheduler, schedulerConfig) ;
                return retryScheduler as IRetryScheduler;
            }
            catch (Exception exception)
            {
                throw new ArgumentException("Invalid Retry scheduler specified in scheduler config", "schedulerConfig",
                    exception);
            }
        }

        /// <summary>
        /// Executes the specified function in the circuit breaker. The ActionResult of this function determines whether the breaker will try to open.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <exception cref="CircuitBreakerTimedOutException">The action timed out </exception>
        /// <exception cref="ArgumentNullException">The value of 'action' cannot be null.</exception>
        /// <exception cref="AggregateException">An exception contained by this <see cref="T:System.AggregateException" /> was not handled.</exception>
        public void Execute(Func<ActionResult> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            EnsureBreakerRegistered();

            HandleOpenBreaker();

            try
            {
                ExecuteTheDelegate(action);
                CloseCircuitBreaker();
            }
            catch (CircuitBreakerTimedOutException)
            {
                OpenBreaker(BreakerOpenReason.Timeout);
                throw;
            }
            catch (AggregateException ae)
            {
                ae.Handle(x =>
                {
                    HandleException(x);
                    return true;
                });
            }
            catch (Exception e)
            {
                HandleException(e);
            }
        }

        /// <summary>
        ///     Executes the specified action in the circuit breaker
        /// </summary>
        /// <param name="action">The action.</param>
        /// <exception cref="CircuitBreakerTimedOutException">The action timed out </exception>
        /// <exception cref="ArgumentNullException">The value of 'action' cannot be null.</exception>
        /// <exception cref="AggregateException">An exception contained by this <see cref="T:System.AggregateException" /> was not handled.</exception>
        public void Execute(Action action)
        {
            Execute(() =>
            {
                action();
                return ActionResult.Good;
            });
        }

        /// <summary>
        ///     Executes the specified function inside the circuit breaker
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="function">The function.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">The value of 'function' cannot be null. </exception>
        /// <exception cref="CircuitBreakerTimedOutException">The action timed out </exception>
        /// <exception cref="AggregateException">An exception contained by this <see cref="T:System.AggregateException" /> was not handled.</exception>
        public T Execute<T>(Func<T> function)
        {
            if (function == null)
                throw new ArgumentNullException("function");
            T result = default(T);
            Execute(() => { result = function(); });
            return result;
        }

        private void ExecuteTheDelegate(Func<ActionResult> action)
        {
            Task<ActionResult> task = new TaskFactory()
                .StartNew(action);

            if (_config.UseTimeout)
            {
                bool didComplete = task.Wait(_config.Timeout);
                if (!didComplete)
                {
                    throw new CircuitBreakerTimedOutException(_config);
                }
                if (task.Result != ActionResult.Good)
                {
                    throw new ActionResultNotGoodException(_config);
                }
            }
            else
            {
                task.Wait();
                if (task.Result != ActionResult.Good)
                {
                    throw new ActionResultNotGoodException(_config);
                }
            }
        }

        private void EnsureBreakerRegistered()
        {
            if (State == BreakerState.Uninitialised)
            {
                if (_useOldEventing)
                {
                    _registerEventHandler.RaiseEvent(_config);
                }
                if (RegisterCircuitBreaker != null)
                {
                    RegisterCircuitBreaker(this, new CircuitBreakerEventArgs(_config));
                }

                CloseCircuitBreaker();
            }
        }

        private void HandleOpenBreaker()
        {
            if (State == BreakerState.Open)
            {
                if (_retryScheduler.AllowRetry)
                {
                    TryToCloseBreaker();
                }
                else
                {
                    throw new CircuitBreakerOpenException(_config);
                }
            }
        }

        private void HandleException(Exception e)
        {
            if (_config.ExpectedExceptionListType == ExceptionListType.WhiteList)
            {
                ProcessWhiteList(e);
            }
            else if (_config.ExpectedExceptionListType == ExceptionListType.BlackList)
            {
                ProcessBlackList(e);
            }
            else
            {
                OpenBreaker(BreakerOpenReason.Exception, e);
                throw new CircuitBreakerExecutionErrorException(_config, e);
            }
        }

        private void ProcessBlackList(Exception e)
        {
            var ae = e as AggregateException;
            bool isBlack = ae == null ? IsListedType(e) : ae.Flatten().InnerExceptions.Any(IsListedType);

            if (isBlack)
            {
                OpenBreaker(BreakerOpenReason.Exception, e);
                throw new CircuitBreakerExecutionErrorException(_config, e);
            }

            if (_config.PermittedExceptionPassThrough == PermittedExceptionBehaviour.PassThrough)
                throw e;
        }

        private void ProcessWhiteList(Exception e)
        {
            var ae = e as AggregateException;
            bool isWhite = ae == null ? IsListedType(e) : ae.Flatten().InnerExceptions.All(IsListedType);

            if (!isWhite)
            {
                OpenBreaker(BreakerOpenReason.Exception, e);
                throw new CircuitBreakerExecutionErrorException(_config, e);
            }
            if (State == BreakerState.HalfOpen)
                CloseCircuitBreaker();

            if(_config.PermittedExceptionPassThrough == PermittedExceptionBehaviour.PassThrough)
                throw e;
        }

        private bool IsListedType(Exception e)
        {
            return _config.ExpectedExceptionList.Contains(e.GetType());
        }

        private void OpenBreaker(BreakerOpenReason reason, Exception thrownException = null)
        {
            if (State != BreakerState.Open)
            {
                if (State == BreakerState.HalfOpen || _toleratedOpenEventCount >= _config.OpenEventTolerance)
                {
                    State = BreakerState.Open;
                    _retryScheduler.BeginNextPeriod(DateTime.UtcNow);
                    OnOpenBreaker(reason, thrownException);
                    _toleratedOpenEventCount = 0;
                }
                else
                {
                    OnToleratedOpenBreaker(_toleratedOpenEventCount++, reason, thrownException);
                }
            }
        }

        private void OnToleratedOpenBreaker(short toleratedOpenEventCount, BreakerOpenReason reason, Exception thrownException)
        {
            if(_useOldEventing)
            {
                _toleratedOpenEventHandler.RaiseEvent(toleratedOpenEventCount, _config, reason, thrownException);
            }
            if (ToleratedOpenCircuitBreaker != null)
            {
                ToleratedOpenCircuitBreaker(this,
                    new ToleratedOpenCircuitBreakerEventArgs(_config, reason, thrownException, toleratedOpenEventCount));
            }
        }

        private void OnOpenBreaker(BreakerOpenReason reason, Exception thrownException)
        {
            if (_useOldEventing)
            {
                _openedEventHandler.RaiseEvent(_config, reason, thrownException);
            }
            if (OpenedCircuitBreaker != null)
            {
                OpenedCircuitBreaker(this, new OpenedCircuitBreakerEventArgs(_config, reason, thrownException));
            }
        }

        private void CloseCircuitBreaker()
        {
            if (State != BreakerState.Closed)
            {
                _retryScheduler.Reset();
                State = BreakerState.Closed;
                if(_useOldEventing)
                {
                    _closedEventHandler.RaiseEvent(_config);
                }
                if (ClosedCircuitBreaker != null)
                {
                    ClosedCircuitBreaker(this, new CircuitBreakerEventArgs(_config));
                }
            }
        }

        private void TryToCloseBreaker()
        {
            if (State != BreakerState.HalfOpen)
            {
                State = BreakerState.HalfOpen;
                if(_useOldEventing)
                {
                    _tryingToCloseEventHandler.RaiseEvent(_config);
                }
                if (TryingToCloseCircuitBreaker != null)
                {
                    TryingToCloseCircuitBreaker(this, new CircuitBreakerEventArgs(_config));
                }
            }
        }

        ~CircuitBreaker()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                if(_useOldEventing)
                {
                    _unregisterEventHandler.RaiseEvent(_config);
                }
                if (UnregisterCircuitBreaker != null)
                {
                    UnregisterCircuitBreaker(this, new CircuitBreakerEventArgs(_config));
                }
            }
        }
    }
}