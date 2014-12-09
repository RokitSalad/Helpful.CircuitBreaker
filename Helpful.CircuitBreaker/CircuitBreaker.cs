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
    public class CircuitBreaker
    {
        internal static Func<ISchedulerConfig, IRetryScheduler> SchedulerActivator;

        private readonly IClosedEvent _closedEventHandler;
        private readonly CircuitBreakerConfig _config;
        private readonly IOpenedEvent _openedEventHandler;
        private readonly IRetryScheduler _retryScheduler;
        private readonly ITolleratedOpenEvent _toleratedOpenEventHandler;
        private readonly ITryingToCloseEvent _tryingToCloseEventHandler;

        private BreakerState _state;
        private short _toleratedOpenEventCount;

        internal CircuitBreaker(
            IClosedEvent closedEventHandler,
            IOpenedEvent openedEventHandler,
            ITryingToCloseEvent tryingToCloseEventHandler,
            ITolleratedOpenEvent toleratedOpenEventHandler,
            CircuitBreakerConfig config
            )
        {
            _closedEventHandler = closedEventHandler;
            _openedEventHandler = openedEventHandler;
            _tryingToCloseEventHandler = tryingToCloseEventHandler;
            _toleratedOpenEventHandler = toleratedOpenEventHandler;
            _config = config;

            if (SchedulerActivator == null)
            {
                _retryScheduler = DefaultSchedulerActivator(config.SchedulerConfig);
            }
            else
            {
                _retryScheduler = SchedulerActivator(config.SchedulerConfig);
            }

            CloseBreaker();
        }

        /// <summary>
        ///     Gets the state.
        /// </summary>
        /// <value>
        ///     The state.
        /// </value>
        public BreakerState State
        {
            get { return _state; }
            internal set { _state = value; }
        }

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
        ///     Executes the specified action in the circuit breaker
        /// </summary>
        /// <param name="action">The action.</param>
        /// <exception cref="CircuitBreakerTimedOutException">The action timed out </exception>
        /// <exception cref="ArgumentNullException">The value of 'action' cannot be null.</exception>
        /// <exception cref="AggregateException">An exception contained by this <see cref="T:System.AggregateException" /> was not handled.</exception>
        public void Execute(Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            HandleOpenBreaker();
            try
            {
                Task task = new TaskFactory()
                    .StartNew(action);

                if (_config.UseTimeout)
                {
                    bool didComplete = task.Wait(_config.Timeout);
                    if (!didComplete)
                    {
                        throw new CircuitBreakerTimedOutException(_config);
                    }
                }
                else
                {
                    task.Wait();
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
                    return false;
                });
            }
            catch (Exception e)
            {
                HandleException(e);
            }
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
            if (_state == BreakerState.Open)
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
            if (_state == BreakerState.HalfOpen)
                CloseBreaker();

            throw e;
        }

        private bool IsListedType(Exception e)
        {
            return _config.ExpectedExceptionList.Contains(e.GetType());
        }

        private void OpenBreaker(BreakerOpenReason reason, Exception thrownException = null)
        {
            if (_state != BreakerState.Open)
            {
                if (_state == BreakerState.HalfOpen || _toleratedOpenEventCount >= _config.OpenEventTolerance)
                {
                    _state = BreakerState.Open;
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
            if (_state != BreakerState.Closed)
            {
                _retryScheduler.Reset();
                _state = BreakerState.Closed;
                _closedEventHandler.RaiseEvent(_config);
            }
        }

        private void TryToCloseBreaker()
        {
            if (_state != BreakerState.HalfOpen)
            {
                _state = BreakerState.HalfOpen;
                _tryingToCloseEventHandler.RaiseEvent(_config);
            }
        }
    }
}