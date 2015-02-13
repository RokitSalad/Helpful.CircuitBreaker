﻿using System;
using Helpful.CircuitBreaker.Events;
using Helpful.CircuitBreaker.Exceptions;

namespace Helpful.CircuitBreaker
{
    public interface ICircuitBreaker : IDisposable
    {
        /// <summary>
        /// Raised when the circuit breaker enters the closed state
        /// </summary>
        event EventHandler<CircuitBreakerEventArgs> ClosedCircuitBreaker;

        /// <summary>
        /// Raised when the circuit breaker enters the opened state
        /// </summary>
        event EventHandler<OpenedCircuitBreakerEventArgs> OpenedCircuitBreaker;

        /// <summary>
        /// Raised when trying to close the circuit breaker
        /// </summary>
        event EventHandler<CircuitBreakerEventArgs> TryingToCloseCircuitBreaker;

        /// <summary>
        /// Raised when the breaker tries to open but remains closed due to tolerance
        /// </summary>
        event EventHandler<ToleratedOpenCircuitBreakerEventArgs> ToleratedOpenCircuitBreaker;

        /// <summary>
        /// Raised when the circuit breaker is disposed
        /// </summary>
        event EventHandler<CircuitBreakerEventArgs> UnregisterCircuitBreaker;

        /// <summary>
        /// Raised when a circuit breaker is first used
        /// </summary>
        event EventHandler<CircuitBreakerEventArgs> RegisterCircuitBreaker;

        /// <summary>
        ///     Gets the state.
        /// </summary>
        /// <value>
        ///     The state.
        /// </value>
        BreakerState State { get; }

        /// <summary>
        /// Gets the breaker identifier.
        /// </summary>
        /// <value>
        /// The breaker identifier.
        /// </value>
        string BreakerId { get; }

        /// <summary>
        /// Executes the specified function in the circuit breaker. The ActionResult of this function determines whether the breaker will try to open.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <exception cref="CircuitBreakerTimedOutException">The action timed out </exception>
        /// <exception cref="ArgumentNullException">The value of 'action' cannot be null.</exception>
        /// <exception cref="AggregateException">An exception contained by this <see cref="T:System.AggregateException" /> was not handled.</exception>
        void Execute(Func<ActionResult> action);

        /// <summary>
        ///     Executes the specified action in the circuit breaker
        /// </summary>
        /// <param name="action">The action.</param>
        /// <exception cref="CircuitBreakerTimedOutException">The action timed out </exception>
        /// <exception cref="ArgumentNullException">The value of 'action' cannot be null.</exception>
        /// <exception cref="AggregateException">An exception contained by this <see cref="T:System.AggregateException" /> was not handled.</exception>
        void Execute(Action action);
    }
}