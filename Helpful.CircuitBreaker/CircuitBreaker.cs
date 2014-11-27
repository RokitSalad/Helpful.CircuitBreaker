using System;
using System.Linq;
using System.Threading.Tasks;
using Helpful.CircuitBreaker.Events;
using Helpful.CircuitBreaker.Exceptions;

namespace Helpful.CircuitBreaker
{
    public class CircuitBreaker
    {
        readonly IClosedEvent _closedEvent;
        readonly IOpenedEvent _openedEvent;
        readonly ITryingToCloseEvent _tryingToCloseEvent;
        readonly ITolleratedOpenEvent _tolleratedOpenEvent;
        readonly CircuitBreakerConfig _config;
        readonly IRetryScheduler _retryScheduler;

        short _tolleratedOpenEventCount;
        
        public BreakerState State { get; private set; }

        public CircuitBreakerConfig Config
        {
            get { return _config; }
        }

        public CircuitBreaker(
            IClosedEvent closedEvent, 
            IOpenedEvent openedEvent, 
            ITryingToCloseEvent tryingToCloseEvent,
            ITolleratedOpenEvent tolleratedOpenEvent, 
            CircuitBreakerConfig config,
            IRetryScheduler retryScheduler)
        {
            _closedEvent = closedEvent;
            _openedEvent = openedEvent;
            _tryingToCloseEvent = tryingToCloseEvent;
            _tolleratedOpenEvent = tolleratedOpenEvent;
            _config = config;
            _retryScheduler = retryScheduler;
            _tolleratedOpenEventCount = 0;

            CloseBreaker();
        }

        public void Execute(Action action)
        {
            HandleOpenBreaker();
            if (_config.UseTimeout)
            {
                ApplyTimeout(action);
            }
            else
            {
                try
                {
                    action();
                    CloseBreaker();
                }
                catch (Exception e)
                {
                    HandleException(e);
                }
            }
        }

        private void HandleOpenBreaker()
        {
            if (State == BreakerState.Open)
            {
                if (!_retryScheduler.AllowRetry)
                {
                    throw new CircuitBreakerOpenException(_config);
                }
                TryToCloseBreaker();
            }
        }

        private void ApplyTimeout(Action action)
        {
            try
            {
                Task actionTask = new Task(action);
                actionTask.Start();
                try
                {
                    if (!actionTask.Wait(_config.Timeout))
                    {
                        OpenBreaker(BreakerOpenReason.Timeout);
                        throw new CircuitBreakerTimedOutException(_config);
                    }
                }
                catch (AggregateException ae)
                {
                    ae.Handle(x => false);
                }
            }
            catch (CircuitBreakerException)
            {
                throw;
            }
            catch (Exception e)
            {
                HandleException(e);
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
            var isBlack = ae == null ? IsListedType(e) : ae.Flatten().InnerExceptions.Any(IsListedType);
            
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
            var isWhite = ae == null ? IsListedType(e) : ae.Flatten().InnerExceptions.All(IsListedType); 
            
            if (!isWhite)
            {
                OpenBreaker(BreakerOpenReason.Exception, e);
                throw new CircuitBreakerExecutionErrorException(_config, e);
            }
            if (State == BreakerState.HalfOpen)
                CloseBreaker();

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
                if (State == BreakerState.HalfOpen || _tolleratedOpenEventCount >= _config.OpenEventTolerance)
                {
                    State = BreakerState.Open;
                    _retryScheduler.BeginNextPeriod(DateTime.UtcNow);
                    _openedEvent.RaiseEvent(_config, reason, thrownException);
                    _tolleratedOpenEventCount = 0;
                }
                else
                {
                    _tolleratedOpenEvent.RaiseEvent(_tolleratedOpenEventCount++, _config, reason, thrownException);
                }
            }
        }

        private void CloseBreaker()
        {
            if (State != BreakerState.Closed)
            {
                _retryScheduler.Reset();
                State = BreakerState.Closed;
                _closedEvent.RaiseEvent(_config);
            }
        }

        private void TryToCloseBreaker()
        {
            if (State != BreakerState.HalfOpen)
            {
                State = BreakerState.HalfOpen;
                _tryingToCloseEvent.RaiseEvent(_config);
            }
        }
    }
}
