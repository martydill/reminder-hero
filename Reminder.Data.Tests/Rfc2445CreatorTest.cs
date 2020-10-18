using NUnit.Framework;
using ReminderHero.Models;
using ReminderHero.Models.Utility;
using Shouldly;

namespace ReminderHero.Data.Tests
{
    class Rfc2445CreatorTest
    {
        [Test]
        public void recur_daily()
        {
            var result = Rfc2445Creator.From(new Reminder() { Recurrence = Recurrence.Daily});
            result.ShouldBe("RRULE:FREQ=DAILY");
        }
       
        [Test]
        public void recur_weekly()
        {
            var result = Rfc2445Creator.From(new Reminder() { Recurrence = Recurrence.Weekly});
            result.ShouldBe("RRULE:FREQ=WEEKLY");
        }
       
        [Test]
        public void recur_monthly()
        {
            var result = Rfc2445Creator.From(new Reminder() { Recurrence = Recurrence.Monthly});
            result.ShouldBe("RRULE:FREQ=MONTHLY");
        }
       
        [Test]
        public void recur_yearly()
        {
            var result = Rfc2445Creator.From(new Reminder() { Recurrence = Recurrence.Yearly});
            result.ShouldBe("RRULE:FREQ=YEARLY");
        }
       
        [Test]
        public void recur_weekend()
        {
            var result = Rfc2445Creator.From(new Reminder() { Recurrence = Recurrence.Weekend});
            result.ShouldBe("RRULE:FREQ=WEEKLY;BYDAY=SA;");
        }
       
        [Test]
        public void recur_weekday()
        {
            var result = Rfc2445Creator.From(new Reminder() { Recurrence = Recurrence.Weekday});
            result.ShouldBe("RRULE:FREQ=WEEKLY;BYDAY=MO,TU,WE,TH,FR;");
        }
       
        [Test]
        public void recur_everyxdays()
        {
            var result = Rfc2445Creator.From(new Reminder() { Recurrence = Recurrence.EveryXDays, RecurrencePeriod = 10});
            result.ShouldBe("RRULE:FREQ=DAILY;INTERVAL=10;");
        }
        
        [Test]
        public void recur_everyxweeks()
        {
            var result = Rfc2445Creator.From(new Reminder() { Recurrence = Recurrence.EveryXWeeks, RecurrencePeriod = 2});
            result.ShouldBe("RRULE:FREQ=WEEKLY;INTERVAL=2;");
        }
        
        [Test]
        public void recur_everyxmonths()
        {
            var result = Rfc2445Creator.From(new Reminder() { Recurrence = Recurrence.EveryXMonths, RecurrencePeriod = 2});
            result.ShouldBe("RRULE:FREQ=MONTHLY;INTERVAL=2;");
        }
        
        [Test]
        public void recur_everyxyears()
        {
            var result = Rfc2445Creator.From(new Reminder() { Recurrence = Recurrence.EveryXYears, RecurrencePeriod = 2});
            result.ShouldBe("RRULE:FREQ=YEARLY;INTERVAL=2;");
        }
    }
}
