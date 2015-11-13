using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Helpful.CircuitBreaker.Config.Sections
{
    public static class Extensions
    {
        public static CircuitBreakerConfig ToBreakerConfig(this CircuitBreakerConfigSection circuitBreakerConfigSection)
        {
            var circuitBreakerConfig = new CircuitBreakerConfig
            {
                BreakerId = string.Format("{0}-{1}", circuitBreakerConfigSection.BreakerId, Environment.MachineName),
                UseTimeout = circuitBreakerConfigSection.UseTimeout,
                OpenEventTolerance = circuitBreakerConfigSection.OpenEventTolerance,
                Timeout = GetTimeout(circuitBreakerConfigSection),
                ExpectedExceptionList = GetExpectedExceptionList(circuitBreakerConfigSection),
                UseImmediateFailureRetry = circuitBreakerConfigSection.UseImmediateFailureRetry
            };
            if (!string.IsNullOrEmpty(circuitBreakerConfigSection.BreakerOpenPeriods))
            {
                circuitBreakerConfig.BreakerOpenPeriods = GetBreakerOpenPeriods(circuitBreakerConfigSection);
            }
            if (circuitBreakerConfigSection.Exceptions.Count > 0)
            {
                circuitBreakerConfig.ExpectedExceptionListType = GetExceptionListType(circuitBreakerConfigSection);
            }
            if (circuitBreakerConfigSection.PermittedExceptionBehaviourConfig != PermittedExceptionBehaviourConfig.None)
            {
                circuitBreakerConfig.PermittedExceptionPassThrough =
                    GetPermittedExceptionBehaviour(circuitBreakerConfigSection);
            }
            if (!string.IsNullOrEmpty(circuitBreakerConfigSection.OpenEventToleranceResetPeriod))
            {
                circuitBreakerConfig.OpenEventToleranceResetPeriod =
                    GetOpenEventToleranceResetPeriod(circuitBreakerConfigSection);
            }
            return circuitBreakerConfig;
        }

        private static TimeSpan GetOpenEventToleranceResetPeriod(CircuitBreakerConfigSection circuitBreakerConfigSection)
        {
            int seconds;
            if (int.TryParse(circuitBreakerConfigSection.OpenEventToleranceResetPeriod, out seconds))
            {
                return TimeSpan.FromSeconds(seconds);
            }
            return TimeSpan.FromSeconds(300);
        }

        private static TimeSpan GetTimeout(CircuitBreakerConfigSection circuitBreakerConfigSection)
        {
            int timeoutSecs;
            if(int.TryParse(circuitBreakerConfigSection.Timeout, out timeoutSecs))
            {
                return TimeSpan.FromSeconds(timeoutSecs);
            }
            return TimeSpan.FromSeconds(0);
        }

        private static TimeSpan[] GetBreakerOpenPeriods(CircuitBreakerConfigSection circuitBreakerConfigSection)
        {
            List<TimeSpan> timeSpans = new List<TimeSpan>();
            if(!string.IsNullOrEmpty(circuitBreakerConfigSection.BreakerOpenPeriods))
            {
                int[] secondsArray = JsonConvert.DeserializeObject<int[]>(circuitBreakerConfigSection.BreakerOpenPeriods);
                foreach (int seconds in secondsArray)
                {
                    timeSpans.Add(TimeSpan.FromSeconds(seconds));
                }
            }
            return timeSpans.ToArray();
        }

        private static List<Type> GetExpectedExceptionList(CircuitBreakerConfigSection circuitBreakerConfigSection)
        {
            if (circuitBreakerConfigSection.Exceptions == null || !circuitBreakerConfigSection.Exceptions.Cast<CircuitBreakerListedExceptionsElement>().Any())
                return new List<Type>();

            return circuitBreakerConfigSection.Exceptions
                .Cast<CircuitBreakerListedExceptionsElement>()
                .Select(e => Type.GetType(e.ExceptionType))
                .ToList();
        }

        private static ExceptionListType GetExceptionListType(CircuitBreakerConfigSection circuitBreakerConfigSection)
        {
            if (circuitBreakerConfigSection.Exceptions == null)
                return ExceptionListType.None;

            switch (circuitBreakerConfigSection.Exceptions.ListTypeConfig)
            {
                case ExceptionListTypeConfig.Blacklist:
                    return ExceptionListType.BlackList;
                case ExceptionListTypeConfig.Whitelist:
                    return ExceptionListType.WhiteList;
                default:
                    return ExceptionListType.None;
            }
        }

        private static PermittedExceptionBehaviour GetPermittedExceptionBehaviour(CircuitBreakerConfigSection circuitBreakerConfigSection)
        {
            switch (circuitBreakerConfigSection.PermittedExceptionBehaviourConfig)
            {
                case PermittedExceptionBehaviourConfig.Swallow:
                    return PermittedExceptionBehaviour.Swallow;
                default:
                    return PermittedExceptionBehaviour.PassThrough;
            }
        }
    }
}
