using System;
using System.Linq;
using System.Threading.Tasks;
using Helpful.CircuitBreaker.Events;
using Helpful.CircuitBreaker.Exceptions;

namespace Helpful.CircuitBreaker
{
    public class CircuitBreaker
    {
        private readonly IClosedEvent _closedEvent;
        private readonly IOpenedEvent _openedEvent;
        private readonly ITryingToCloseEvent _tryingToCloseEvent;
        private readonly ITolleratedOpenEvent _tolleratedOpenEvent;
        private readonly CircuitBreakerConfig _config;

        private short _tolleratedOpenEventCount;
        private DateTime _timeOpened;

        public BreakerState State { get; private set; }

        public CircuitBreakerConfig Config
        {
            get { return _config; }
        }

        public CircuitBreaker(IClosedEvent closedEvent, IOpenedEvent openedEvent, ITryingToCloseEvent tryingToCloseEvent,
            ITolleratedOpenEvent tolleratedOpenEvent, CircuitBreakerConfig config)
        {
            _closedEvent = closedEvent;
            _openedEvent = openedEvent;
            _tryingToCloseEvent = tryingToCloseEvent;
            _tolleratedOpenEvent = tolleratedOpenEvent;
            _config = config;
            _tolleratedOpenEventCount = 0;
            CloseBreaker();
        }

        public void Execute(Action action)
        {
            HandleClosedBreaker();
            if(_config.UseTimeout)
            {
                ApplyTimeout(action);
            }
            else
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    HandleException(e);
                }
            }
        }

        private void HandleClosedBreaker()
        {
            if (State == BreakerState.Open)
            {
                if (_timeOpened.Add(TimeSpan.FromSeconds(_config.RetryPeriodInSeconds)) > DateTime.Now)
                {
                    throw new CircuitBreakerOpenException(_config);
                }
                _tryingToCloseEvent.RaiseEvent(_config);
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
            bool isBlack = _config.ExpectedExceptionList.Any(exType => e.GetType() == exType);
            if (isBlack)
            {
                OpenBreaker(BreakerOpenReason.Exception, e);
                throw new CircuitBreakerExecutionErrorException(_config, e);
            }
            throw e;
        }

        private void ProcessWhiteList(Exception e)
        {
            bool isWhite = _config.ExpectedExceptionList.Any(exType => e.GetType() == exType);
            if (!isWhite)
            {
                OpenBreaker(BreakerOpenReason.Exception, e);
                throw new CircuitBreakerExecutionErrorException(_config, e);
            }
            throw e;
        }

        private void OpenBreaker(BreakerOpenReason reason, Exception thrownException = null)
        {
            if(_tolleratedOpenEventCount >= _config.OpenEventTollerance)
            {
                State = BreakerState.Open;
                _timeOpened = DateTime.Now;
                _openedEvent.RaiseEvent(_config, reason, thrownException);
                _tolleratedOpenEventCount = 0;
            }
            else
            {
                _tolleratedOpenEvent.RaiseEvent(_tolleratedOpenEventCount++, _config, reason, thrownException);
            }
        }

        private void CloseBreaker()
        {
            State = BreakerState.Closed;
            _closedEvent.RaiseEvent(_config);
        }
    }
}
