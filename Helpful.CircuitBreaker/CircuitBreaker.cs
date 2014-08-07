﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Helpful.CircuitBreaker.Events;
using Helpful.CircuitBreaker.Exceptions;

namespace Helpful.CircuitBreaker
{
    public class CircuitBreaker
    {
        private BreakerState _state;
        private short _tolleratedOpenEventCount;
        private readonly IClosedEvent _closedEvent;
        private readonly IOpenedEvent _openedEvent;
        private readonly ITriedToCloseEvent _triedToCloseEvent;
        private readonly ITolleratedOpenEvent _tolleratedOptEvent;
        private readonly CircuitBreakerConfig _config;

        public CircuitBreakerConfig Config
        {
            get { return _config; }
        }

        public CircuitBreaker(IClosedEvent closedEvent, IOpenedEvent openedEvent, ITriedToCloseEvent triedToCloseEvent,
            ITolleratedOpenEvent tolleratedOptEvent, CircuitBreakerConfig config)
        {
            _closedEvent = closedEvent;
            _openedEvent = openedEvent;
            _triedToCloseEvent = triedToCloseEvent;
            _tolleratedOptEvent = tolleratedOptEvent;
            _config = config;
            _tolleratedOpenEventCount = 0;
            CloseBreaker();
        }

        public void Execute(Action action)
        {
            if(_config.UseTimeout)
            {
                ApplyTimeout(action);
            }
        }

        private void ApplyTimeout(Action action)
        {
            try
            {
                Task actionTask = new Task(action);
                if (!actionTask.Wait(_config.Timeout))
                {
                    OpenBreaker(BreakerOpenReason.Timeout);
                    throw new CircuitBreakerTimedOutException(_config);
                }
            }
            catch (CircuitBreakerException)
            {
                throw;
            }
            catch (Exception e)
            {
                if (_config.UseExceptionWhiteList)
                {
                    ProcessWhiteList(e);
                }
                else if (_config.UseExceptionBlackList)
                {
                    ProcessBlackList(e);
                }
                else
                {
                    OpenBreaker(BreakerOpenReason.Exception, e);
                    throw new CircuitBreakerExecutionErrorException(_config, e);
                }
            }
        }

        private void ProcessBlackList(Exception e)
        {
            bool isBlack = _config.ExceptionBlackList.Any(exType => e.GetType() == exType);
            if (isBlack)
            {
                OpenBreaker(BreakerOpenReason.Exception, e);
                throw new CircuitBreakerExecutionErrorException(_config, e);
            }
        }

        private void ProcessWhiteList(Exception e)
        {
            bool isWhite = _config.ExceptionWhiteList.Any(exType => e.GetType() == exType);
            if (!isWhite)
            {
                OpenBreaker(BreakerOpenReason.Exception, e);
                throw new CircuitBreakerExecutionErrorException(_config, e);
            }
        }

        private void OpenBreaker(BreakerOpenReason reason, Exception thrownException = null)
        {
            if(_tolleratedOpenEventCount >= _config.OpenEventTollerance)
            {
                _openedEvent.RaiseEvent(_config, reason, thrownException);
            }
            else
            {
                _tolleratedOptEvent.RaiseEvent(_tolleratedOpenEventCount++, _config, reason, thrownException);
            }
        }

        private void CloseBreaker()
        {
            _state = BreakerState.Closed;
            _closedEvent.RaiseEvent(_config);
        }
    }
}
