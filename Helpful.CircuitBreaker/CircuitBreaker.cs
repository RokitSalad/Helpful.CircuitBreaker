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
        private readonly CircuitBreakerConfig _config;
        private readonly IOpenedEvent _openedEventHandler;
        private readonly IRetryScheduler _retryScheduler;
        private readonly ITolleratedOpenEvent _toleratedOpenEventHandler;
        private readonly ITryingToCloseEvent _tryingToCloseEventHandler;
        private readonly IUnregisterBreakerEvent _unregisterEventHandler;
        private bool _disposed;

        private short _toleratedOpenEventCount;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="eventFactory">The factory for generating event handlers</param>
        /// <param name="config">The config for the breaker</param>
        public CircuitBreaker(
            IEventFactory eventFactory,
            CircuitBreakerConfig config
            )
        {
            _closedEventHandler = eventFactory.GetClosedEvent();
            _openedEventHandler = eventFactory.GetOpenedEvent();
            _tryingToCloseEventHandler = eventFactory.GetTriedToCloseEvent();
            _toleratedOpenEventHandler = eventFactory.GetTolleratedOpenEvent();
            _unregisterEventHandler = eventFactory.GetUnregisterBreakerEvent();
            _config = config;

            _retryScheduler = SchedulerActivator == null
                                  ? DefaultSchedulerActivator(config.SchedulerConfig)
                                  : SchedulerActivator(config.SchedulerConfig);

            CloseBreaker();
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

        public void Execute(Func<ActionResult> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            HandleOpenBreaker();
            try
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
                CloseBreaker();
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
                CloseBreaker();

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
                    _openedEventHandler.RaiseEvent(_config, reason, thrownException);
                    _toleratedOpenEventCount = 0;
                }
                else
                {
                    _toleratedOpenEventHandler.RaiseEvent(_toleratedOpenEventCount++, _config, reason, thrownException);
                }
            }
        }

        private void CloseBreaker()
        {
            if (State != BreakerState.Closed)
            {
                _retryScheduler.Reset();
                State = BreakerState.Closed;
                _closedEventHandler.RaiseEvent(_config);
            }
        }

        private void TryToCloseBreaker()
        {
            if (State != BreakerState.HalfOpen)
            {
                State = BreakerState.HalfOpen;
                _tryingToCloseEventHandler.RaiseEvent(_config);
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
                _unregisterEventHandler.RaiseEvent(_config);
            }
        }
    }
}