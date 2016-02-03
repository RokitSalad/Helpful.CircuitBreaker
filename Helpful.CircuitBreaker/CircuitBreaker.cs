﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Helpful.CircuitBreaker.Config;
using Helpful.CircuitBreaker.Events;
using Helpful.CircuitBreaker.Exceptions;

namespace Helpful.CircuitBreaker
{
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
                if (!_config.ImmediateRetryOnFailure)
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

        /// <summary>
        /// Executes the specified async action in the circuit breaker. The ActionResult of this function determines whether the breaker will try to open.
        /// </summary>
        /// <param name="asyncAction">A function returning the async action to execute.</param>
        /// <exception cref="T:Helpful.CircuitBreaker.Exceptions.CircuitBreakerTimedOutException">The action timed out </exception>
        /// <exception cref="T:System.ArgumentNullException">The value of 'asyncAction' cannot be null.</exception>
        /// <exception cref="T:System.AggregateException">An exception contained by this <see cref="T:System.AggregateException"/> was not handled.</exception>
        /// <returns>An awaitable task</returns>
        public async Task ExecuteAsync(Func<Task<ActionResult>> asyncAction)
        {
            await ExecuteAsync(asyncAction, new CancellationTokenSource());
        }

        /// <summary>
        /// Executes the specified async action in the circuit breaker. The ActionResult of this function determines whether the breaker will try to open.
        /// </summary>
        /// <param name="asyncAction">A function returning the async action to execute.</param>
        /// <param name="cancellationTokenSource">Required to support task cancellation.</param>
        /// <exception cref="T:Helpful.CircuitBreaker.Exceptions.CircuitBreakerTimedOutException">The action timed out </exception>
        /// <exception cref="T:System.ArgumentNullException">The value of 'asyncAction' cannot be null.</exception>
        /// <exception cref="T:System.AggregateException">An exception contained by this <see cref="T:System.AggregateException"/> was not handled.</exception>
        /// <returns>An awaitable task</returns>
        public async Task ExecuteAsync(Func<Task<ActionResult>> asyncAction, CancellationTokenSource cancellationTokenSource)
        {
            if (asyncAction == null)
                throw new ArgumentNullException("asyncAction");

            EnsureBreakerRegistered();

            HandleOpenBreaker();

            try
            {
                if (!_config.ImmediateRetryOnFailure)
                {
                    await ExecuteTheActionAsync(asyncAction, cancellationTokenSource);
                }
                else
                {
                    await ExecuteTheActionWithImmediateRetryAsync(asyncAction, cancellationTokenSource);
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
                ae.Handle(e =>
                {
                    HandleException(e);
                    return true;
                });
            }
            catch (Exception e)
            {
                HandleException(e);
            }
        }

        /// <summary>
        /// Executes the specified action in the circuit breaker
        /// </summary>
        /// <param name="asyncAction">A function returning the async action to execute.</param>
        /// <exception cref="T:Helpful.CircuitBreaker.Exceptions.CircuitBreakerTimedOutException">The action timed out </exception>
        /// <exception cref="T:System.ArgumentNullException">The value of 'asyncAction' cannot be null.</exception>
        /// <exception cref="T:System.AggregateException">An exception contained by this <see cref="T:System.AggregateException"/> was not handled.</exception>
        /// <returns>An awaitable task</returns>
        public async Task ExecuteAsync(Func<Task> asyncAction)
        {
            Func<Task<ActionResult>> wrapper = async () =>
            {
                await asyncAction();
                return ActionResult.Good;
            };

            await ExecuteAsync(wrapper, new CancellationTokenSource());
        }

        /// <summary>
        /// Executes the specified action in the circuit breaker
        /// </summary>
        /// <param name="asyncAction">A function returning the async action to execute.</param>
        /// <param name="cancellationTokenSource">Required to support task cancellation.</param>
        /// <exception cref="T:Helpful.CircuitBreaker.Exceptions.CircuitBreakerTimedOutException">The action timed out </exception>
        /// <exception cref="T:System.ArgumentNullException">The value of 'asyncAction' cannot be null.</exception>
        /// <exception cref="T:System.AggregateException">An exception contained by this <see cref="T:System.AggregateException"/> was not handled.</exception>
        /// <returns>An awaitable task</returns>
        public async Task ExecuteAsync(Func<Task> asyncAction, CancellationTokenSource cancellationTokenSource)
        {
            Func<Task<ActionResult>> wrapper = async () =>
            {
                await asyncAction();
                return ActionResult.Good;
            };

            await ExecuteAsync(wrapper, cancellationTokenSource);
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

        private async Task ExecuteTheActionAsync(Func<Task<ActionResult>> asyncActionProvider, CancellationTokenSource cancellationTokenSource)
        {
            ActionResult result;

            var asyncAction = asyncActionProvider();

            if (_config.UseTimeout)
            {
                var delayTask = Task.Delay(_config.Timeout, cancellationTokenSource.Token);
                var completedTask = await Task.WhenAny(asyncAction, delayTask);
                cancellationTokenSource.Cancel(false);

                if (completedTask != asyncAction)
                {
                    throw new CircuitBreakerTimedOutException(_config);
                }

                result = asyncAction.Result;
            }
            else
            {
                result = await asyncAction;
            }

            if (result == ActionResult.Failure)
            {
                throw new ActionResultNotGoodException(_config);
            }
        }

        private async Task ExecuteTheActionWithImmediateRetryAsync(Func<Task<ActionResult>> asyncActionProvider, CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                await ExecuteTheActionAsync(asyncActionProvider, cancellationTokenSource);
            }
            catch (Exception)
            {
                await ExecuteTheActionAsync(asyncActionProvider, cancellationTokenSource);
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
                    }
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