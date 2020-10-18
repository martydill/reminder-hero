using System;
using NUnit.Framework;
using ReminderHero.Parser;
using Shouldly;

namespace ReminderHero.Data.Tests
{
    public class RecurringReminderParseTest
    {
        [Test]
        public void no_recurrence_by_default()
        {
            var result = ReminderParser.Parse("do stuff in 2 days", DateTime.Now);
            result.Recurrence.ShouldBe(Models.Recurrence.Once);
        }

        [Test]
        public void weekly_recurrence_including_this_week()
        {
            var now = new DateTime(2013, 09, 25, 06, 0, 0);
            var result = ReminderParser.Parse("do stuff every friday at 2pm", now);
            result.Recurrence.ShouldBe(Models.Recurrence.Weekly);
            result.ReminderDate.Hour.ShouldBe(14);
            result.ReminderDate.Day.ShouldBe(27);
        }

        [Test]
        public void weekly_recurrence_not_including_this_week()
        {
            var now = new DateTime(2013, 09, 26, 06, 0, 0);
            var result = ReminderParser.Parse("do stuff every monday at 2pm", now);
            result.Recurrence.ShouldBe(Models.Recurrence.Weekly);
            result.ReminderDate.Hour.ShouldBe(14);
            result.ReminderDate.Day.ShouldBe(30);
        }

        [Test]
        public void daily_recurrence_including_today()
        {
            var now = new DateTime(2013, 09, 25, 06, 0, 0);
            var result = ReminderParser.Parse("do stuff every day at 2pm", now);
            result.Recurrence.ShouldBe(Models.Recurrence.Daily);
            result.ReminderDate.Hour.ShouldBe(14);
            result.ReminderDate.Day.ShouldBe(25);
        }

        [Test]
        public void daily_recurrence_not_including_today()
        {
            var now = new DateTime(2013, 09, 25, 16, 0, 0);
            var result = ReminderParser.Parse("do stuff every day at 2pm", now);
            result.Recurrence.ShouldBe(Models.Recurrence.Daily);
            result.ReminderDate.Hour.ShouldBe(14);
            result.ReminderDate.Day.ShouldBe(26);
        }

        [Test]
        public void daily_recurrence_with_word_daily()
        {
            var now = new DateTime(2013, 09, 25, 06, 0, 0);
            var result = ReminderParser.Parse("Post Tech News daily at 2 pm.", now);
            result.Recurrence.ShouldBe(Models.Recurrence.Daily);
            result.Description.ShouldBe("Post Tech News");  
            result.ReminderDate.Hour.ShouldBe(14);
            result.ReminderDate.Day.ShouldBe(25);
        }

        [TestCase("morning", ReminderParser.MorningHour)]
        [TestCase("afternoon", ReminderParser.AfternoonHour)]
        [TestCase("evening", ReminderParser.EveningHour)]
        [TestCase("night", ReminderParser.NightHour)]
        public void daily_morning_recurrence(string time, int hour)
        {
            var result = ReminderParser.Parse("do stuff every " + time, DateTime.Now);
            result.Recurrence.ShouldBe(Models.Recurrence.Daily);
            result.ReminderDate.Hour.ShouldBe(hour);
        }

        [Test]
        public void weekday_recurrence_for_next_week()
        {
            var now = new DateTime(2013, 09, 27, 14, 0, 0);
            var result = ReminderParser.Parse("do stuff every weekday at 1pm", now);
            result.Recurrence.ShouldBe(Models.Recurrence.Weekday);
            result.ReminderDate.Day.ShouldBe(30);
            result.ReminderDate.Hour.ShouldBe(13);
        }

        [Test]
        public void weekday_recurrence_for_this_week_including_today()
        {
            var now = new DateTime(2013, 09, 25, 06, 0, 0);
            var result = ReminderParser.Parse("do stuff every weekday at 1pm", now);
            result.Recurrence.ShouldBe(Models.Recurrence.Weekday);
            result.ReminderDate.ShouldBe(now.AddHours(7));
        }

        [Test]
        public void weekday_recurrence_for_this_week_not_including_today()
        {
            var now = new DateTime(2013, 09, 25, 20, 0, 0);
            var result = ReminderParser.Parse("do stuff every weekday at 1pm", now);
            result.Recurrence.ShouldBe(Models.Recurrence.Weekday);
            result.ReminderDate.ShouldBe(new DateTime(2013, 09, 26, 13, 0, 0));
        }

        [Test]
        public void weekend_recurrence()
        {
            var result = ReminderParser.Parse("do stuff every weekend at 2pm", DateTime.Now);
            result.Recurrence.ShouldBe(Models.Recurrence.Weekend);
        }

        // the last friday of every month
        // the xx of every month

        [Test]
        public void every_x_days_recurrence()
        {
            var now = new DateTime(2013, 09, 25, 20, 0, 0);
            var result = ReminderParser.Parse("water the plants every 14 days", now);
            result.Recurrence.ShouldBe(Models.Recurrence.EveryXDays);
            result.RecurrencePeriod.ShouldBe(14);
            result.ReminderDate.ShouldBe(new DateTime(2013, 10, 9, 20, 0, 0));
            result.Description.ShouldBe("water the plants");
        }

        [Test]
        public void every_x_days_with_days_before_message()
        {
            var now = new DateTime(2013, 09, 25, 20, 0, 0);
            var result = ReminderParser.Parse("every 14 days water the plants", now);
            result.Recurrence.ShouldBe(Models.Recurrence.EveryXDays);
            result.RecurrencePeriod.ShouldBe(14);
            result.ReminderDate.ShouldBe(new DateTime(2013, 10, 9, 20, 0, 0));
            result.Description.ShouldBe("water the plants");
        }
        [Test]
        public void every_x_weeks_recurrence()
        {
            var now = new DateTime(2013, 09, 25, 20, 0, 0);
            var result = ReminderParser.Parse("water the plants every three weeks", now);
            result.Recurrence.ShouldBe(Models.Recurrence.EveryXWeeks);
            result.RecurrencePeriod.ShouldBe(3);
            result.ReminderDate.ShouldBe(new DateTime(2013, 10, 16, 20, 0, 0));
            result.Description.ShouldBe("water the plants");
        }

        [Test]
        public void every_x_months_recurrence()
        {
            var now = new DateTime(2013, 09, 25, 20, 0, 0);
            var result = ReminderParser.Parse("water the plants every 2 months", now);
            result.Recurrence.ShouldBe(Models.Recurrence.EveryXMonths);
            result.RecurrencePeriod.ShouldBe(2);
            result.ReminderDate.ShouldBe(new DateTime(2013, 11, 25, 20, 0, 0));
            result.Description.ShouldBe("water the plants");
        }

        [Test]
        public void every_x_years_recurrence()
        {
            var now = new DateTime(2013, 09, 25, 20, 0, 0);
            var result = ReminderParser.Parse("water the plants every 2 years", now);
            result.Recurrence.ShouldBe(Models.Recurrence.EveryXYears);
            result.RecurrencePeriod.ShouldBe(2);
            result.ReminderDate.ShouldBe(new DateTime(2015, 9, 25, 20, 0, 0));
            result.Description.ShouldBe("water the plants");
        }

        [Test]
        public void yearly_recurrence_including_this_year()
        {
            var now = new DateTime(2013, 09, 25, 13, 0, 0);
            var result = ReminderParser.Parse("do stuff every year on nov 10", now);
            result.Recurrence.ShouldBe(Models.Recurrence.Yearly);
            result.ReminderDate.Year.ShouldBe(2013);
            result.ReminderDate.Month.ShouldBe(11);
            result.ReminderDate.Day.ShouldBe(10);
            result.ReminderDate.Hour.ShouldBe(13);
        }

        [Test]
        public void yearly_recurrence_not_including_this_year()
        {
            var now = new DateTime(2013, 09, 26, 06, 0, 0);
            var result = ReminderParser.Parse("do stuff every year on feb 9", now);
            result.Recurrence.ShouldBe(Models.Recurrence.Yearly);
            result.ReminderDate.Year.ShouldBe(2014);
            result.ReminderDate.Month.ShouldBe(2);
            result.ReminderDate.Day.ShouldBe(9);
            result.ReminderDate.Hour.ShouldBe(6);
        }
    }
}
