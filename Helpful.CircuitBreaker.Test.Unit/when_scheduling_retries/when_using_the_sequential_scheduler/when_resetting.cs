using System;
using Helpful.BDD;
using Helpful.CircuitBreaker.Schedulers;
using NUnit.Framework;

namespace when_scheduling_retries.when_using_the_sequential_scheduler
{
    class when_resetting : TestBase
    {
        SequentialRetryScheduler _scheduler;
        DateTime _seedTime;

        protected override void Given()
        {
            base.Given();
            _scheduler = new SequentialRetryScheduler(new[] { 1, 2, 3 });
            _seedTime = DateTime.UtcNow;
        }

        protected override void When()
        {
            _scheduler.BeginNextPeriod(_seedTime);
            _scheduler.BeginNextPeriod(_seedTime);
            _scheduler.Reset();
            _scheduler.BeginNextPeriod(_seedTime);
        }

        [Then]
        public void begins_the_sequence_again()
        {
            Assert.That(_scheduler.CurrentRetryPeriod, Is.EqualTo(TimeSpan.FromSeconds(1)));
            Assert.That(_scheduler.AllowedRetryTime, Is.EqualTo(_seedTime.AddSeconds(1)));
        }
    }
}
