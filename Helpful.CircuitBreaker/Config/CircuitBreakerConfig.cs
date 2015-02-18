using System;
using System.Collections.Generic;
using Helpful.CircuitBreaker.Events;

namespace Helpful.CircuitBreaker.Config
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class CircuitBreakerConfig : ICircuitBreakerDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CircuitBreakerConfig"/> class.
        /// </summary>
        public CircuitBreakerConfig()
        {
            ExpectedExceptionList = new List<Type>();
            ExpectedExceptionListType = ExceptionListType.None;
            PermittedExceptionPassThrough = PermittedExceptionBehaviour.PassThrough;
            BreakerOpenPeriods = new[] { TimeSpan.FromSeconds(60) };
        }

        /// <summary>
        /// The number of times an exception can occur before the circuit breaker is opened
        /// </summary>
        /// <value>
        /// The open event tolerance.
        /// </value>
        public short OpenEventTolerance { get; set; }

        /// <summary>
        /// Gets or sets the list of periods the breaker should be kept open. 
        /// The last value will be what is repeated until the breaker is successfully closed.
        /// If not set, a default of 60 seconds will be used for all breaker open periods.
        /// </summary>
        /// <value>
        /// The array of timespans representing the breaker open periods.
        /// </value>
        public TimeSpan[] BreakerOpenPeriods { get; set; }

        /// <summary>
        /// Gets or sets the expected type of the exception list. <see cref="ExceptionListType"/>
        /// </summary>
        /// <value>
        /// The expected type of the exception list.
        /// </value>
        public ExceptionListType ExpectedExceptionListType { get; set; }

        /// <summary>
        /// Gets or sets the expected exception list.
        /// </summary>
        /// <value>
        /// The expected exception list.
        /// </value>
        public List<Type> ExpectedExceptionList { get; set; }

        /// <summary>
        /// Gets or sets the timeout.
        /// </summary>
        /// <value>
        /// The timeout.
        /// </value>
        public TimeSpan Timeout { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use timeout].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use timeout]; otherwise, <c>false</c>.
        /// </value>
        public bool UseTimeout { get; set; }

        /// <summary>
        /// Gets or sets the breaker identifier.
        /// </summary>
        /// <value>
        /// The breaker identifier.
        /// </value>
        public string BreakerId { get; set; }

        /// <summary>
        /// Sets the behaviour for passing through exceptions that won't open the breaker
        /// </summary>
        public PermittedExceptionBehaviour PermittedExceptionPassThrough { get; set; }
    }
}