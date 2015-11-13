using System;
using System.Linq;
using System.Threading.Tasks;
using Helpful.CircuitBreaker.Config;
using Helpful.CircuitBreaker.Events;
using Helpful.CircuitBreaker.Exceptions;

namespace Helpful.CircuitBreaker
{
    using System.Threading;

    /// <summary>
    /// </summary>
    public class CircuitBreaker : ICircuitBreaker
    {
        private readonly CircuitBreakerConfig _config;

        private bool _disposed;
        private int _openPeriodIndex;
        private DateTime _openedTime;
        private volatile int _toleratedOpenEventCount;
        private DateTime _firstToleratedEventTime;

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

        private bool CanTryToCloseBreaker
        {
            get { return _openedTime + _config.BreakerOpenPeriods[_openPeriodIndex] <= DateTime.UtcNow; }
        }

        private bool CanResetToleratedEventCount
        {
            get { return _firstToleratedEventTime != DateTime.MinValue && _firstToleratedEventTime + _config.OpenEventToleranceResetPeriod <= DateTime.UtcNow; }
        }

        /// <summary>
        /// Constructor without an event factory - the breaker will just raise normal .Net events for you to handle
        /// </summary>
        /// <param name="config">The config for the breaker</param>
        public CircuitBreaker(CircuitBreakerConfig config)
        {
            _config = config;
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
                if (!_config.UseImmediateFailureRetry)
                {
                    ExecuteTheDelegate(action);
                }
                else
                {
                    ExecuteTheDelegateWithImmediateRetry(action);
                }
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

        private void ExecuteTheDelegateWithImmediateRetry(Func<ActionResult> action)
        {
            try
            {
                ExecuteTheDelegate(action);
            }
            catch
            {
                ExecuteTheDelegate(action);
            }
        }

        private void EnsureBreakerRegistered()
        {
            if (State == BreakerState.Uninitialised)
            {
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
                if (CanTryToCloseBreaker)
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
                if (State == BreakerState.HalfOpen)
                {
                    SafeIncrementOpenPeriodIndex();
                }
                if (CanResetToleratedEventCount)
                {
                    _toleratedOpenEventCount = 0;
                    _firstToleratedEventTime = DateTime.MinValue;
                }
                if (State == BreakerState.HalfOpen || _toleratedOpenEventCount >= _config.OpenEventTolerance)
                {
                    State = BreakerState.Open;
                    _openedTime = DateTime.UtcNow;
                    OnOpenBreaker(reason, thrownException);
                    _toleratedOpenEventCount = 0;
                }
                else
                {
                    OnToleratedOpenBreaker(_toleratedOpenEventCount++, reason, thrownException);
                    if (_toleratedOpenEventCount == 1)
                    {
                        _firstToleratedEventTime = DateTime.UtcNow;
                    };
                }
            }
        }

        private void SafeIncrementOpenPeriodIndex()
        {
            _openPeriodIndex = _openPeriodIndex == _config.BreakerOpenPeriods.Length - 1
                ? _openPeriodIndex
                : _openPeriodIndex + 1;
        }

        private void OnToleratedOpenBreaker(int toleratedOpenEventCount, BreakerOpenReason reason, Exception thrownException)
        {
            if (ToleratedOpenCircuitBreaker != null)
            {
                ToleratedOpenCircuitBreaker(this,
                    new ToleratedOpenCircuitBreakerEventArgs(_config, reason, thrownException, toleratedOpenEventCount));
            }
        }

        private void OnOpenBreaker(BreakerOpenReason reason, Exception thrownException)
        {
            if (OpenedCircuitBreaker != null)
            {
                OpenedCircuitBreaker(this, new OpenedCircuitBreakerEventArgs(_config, reason, thrownException));
            }
        }

        private void CloseCircuitBreaker()
        {
            if (State != BreakerState.Closed)
            {
                _openPeriodIndex = 0;
                State = BreakerState.Closed;
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
                if (UnregisterCircuitBreaker != null)
                {
                    UnregisterCircuitBreaker(this, new CircuitBreakerEventArgs(_config));
                }
            }
        }
    }
}