using System;
using System.Collections.Generic;

namespace Helpful.CircuitBreaker.Config
{
    public interface ICircuitBreakerDefinition
    {
        /// <summary>
        /// The number of times an exception can occur before the circuit breaker is opened
        /// </summary>
        /// <value>
        /// The open event tolerance.
        /// </value>
        short OpenEventTolerance { get; set; }

        /// <summary>
        /// Gets or sets the list of periods the breaker should be kept open. 
        /// The last value will be what is repeated until the breaker is successfully closed.
        /// If not set, a default of 60 seconds will be used for all breaker open periods.
        /// </summary>
        /// <value>
        /// The array of timespans representing the breaker open periods.
        /// </value>
        TimeSpan[] BreakerOpenPeriods { get; set; }

        /// <summary>
        /// Gets or sets the expected type of the exception list. <see cref="ExceptionListType"/>
        /// </summary>
        /// <value>
        /// The expected type of the exception list.
        /// </value>
        ExceptionListType ExpectedExceptionListType { get; set; }

        /// <summary>
        /// Gets or sets the expected exception list.
        /// </summary>
        /// <value>
        /// The expected exception list.
        /// </value>
        List<Type> ExpectedExceptionList { get; set; }

        /// <summary>
        /// Gets or sets the timeout.
        /// </summary>
        /// <value>
        /// The timeout.
        /// </value>
        TimeSpan Timeout { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use timeout].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use timeout]; otherwise, <c>false</c>.
        /// </value>
        bool UseTimeout { get; set; }

        /// <summary>
        /// Gets or sets the breaker identifier.
        /// </summary>
        /// <value>
        /// The breaker identifier.
        /// </value>
        string BreakerId { get; set; }

        /// <summary>
        /// Sets the behaviour for passing through exceptions that won't open the breaker
        /// </summary>
        PermittedExceptionBehaviour PermittedExceptionPassThrough { get; set; }
    }
}