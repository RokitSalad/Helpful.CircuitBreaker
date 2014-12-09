﻿namespace when_scheduling_retries.when_using_the_fixed_scheduler
{
    using System;
    using Helpful.BDD;
    using Helpful.CircuitBreaker.Config;
    using Helpful.CircuitBreaker.Schedulers;
    using NUnit.Framework;

    [Category("Unit")]
    class when_beginning_the_first_period : TestBase
    {
        FixedRetryScheduler _scheduler;
        DateTime _seedTime;

        protected override void Given()
        {
            base.Given();
            _scheduler = new FixedRetryScheduler(new FixedRetrySchedulerConfig { RetryPeriodInSeconds = 10 });
            _seedTime = DateTime.UtcNow;
        }

        protected override void When()
        {
            _scheduler.BeginNextPeriod(_seedTime);
        }

        [Then]
        public void uses_the_configured_retry_value()
        {
            Assert.That(_scheduler.AllowedRetryTime, Is.EqualTo(_seedTime.AddSeconds(10)));
        }
    }
}
