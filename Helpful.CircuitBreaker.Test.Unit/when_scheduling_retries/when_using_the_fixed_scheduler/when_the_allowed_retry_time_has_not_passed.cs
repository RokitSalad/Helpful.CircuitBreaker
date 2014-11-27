using System;
using Helpful.BDD;
using Helpful.CircuitBreaker.Schedulers;
using NUnit.Framework;

namespace when_scheduling_retries.when_using_the_fixed_scheduler
{
    class when_the_allowed_retry_time_has_not_passed : TestBase
    {
        FixedRetryScheduler _scheduler;
        DateTime _seedTime;

        protected override void Given()
        {
            base.Given();
            _scheduler = new FixedRetryScheduler(10);
            _seedTime = DateTime.UtcNow.AddSeconds(-9);
        }

        protected override void When()
        {
            _scheduler.BeginNextPeriod(_seedTime);
        }

        [Then]
        public void does_not_allow_retry()
        {
            Assert.That(_scheduler.AllowRetry, Is.False);
        }
    }
}
