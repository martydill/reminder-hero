using System;
using NUnit.Framework;
using ReminderHero.Models;
using Shouldly;

namespace ReminderHero.Data.Tests
{
    public class ReminderTests
    {
        [Test]
        public void recur_resets_reminders()
        {
            var r = new Reminder();
            r.Recurrence = Recurrence.Daily;
            r.Reminder1Fired = true;
            r.Reminder2Fired = true;
            r.Reminder3Fired = true;
            r.Reminder4Fired = true;
            var user = new User()
            {
                TimeZone = "America/Los_Angeles",
                Reminder1Number = 1,
                Reminder1Period = ReminderPeriod.Minutes,
                Reminder1Enabled = true,
                Reminder2Number = 2,
                Reminder2Period = ReminderPeriod.Hours,
                Reminder2Enabled = true,
                Reminder3Number = 3,
                Reminder3Period = ReminderPeriod.Days,
                Reminder3Enabled = true,
                Reminder4Number = 4,
                Reminder4Period = ReminderPeriod.Weeks,
                Reminder4Enabled = true
            };
            r.User = user;
            DateTime date = new DateTime(2013, 08, 10, 2, 30, 0);
            r.ReminderDate = date;

            r.HandleDelivery(HandleReminderType.Real);
            r.Reminder1Fired.ShouldBe(false);
            r.Reminder2Fired.ShouldBe(false);
            r.Reminder3Fired.ShouldBe(false);
            r.Reminder4Fired.ShouldBe(false);
        }

        [Test]
        public void recur_marks_past_time_reminders_as_fired()
        {
            var r = new Reminder();
            r.Recurrence = Recurrence.Daily;
            r.Reminder1Fired = true;
            r.Reminder2Fired = true;
            r.Reminder3Fired = true;
            r.Reminder4Fired = true;

            var user = new User()
            {
                TimeZone = "America/Los_Angeles",
                Reminder1Number = 100,
                Reminder1Period = ReminderPeriod.Minutes,
                Reminder1Enabled = true,
                Reminder2Number = 10 * 60,
                Reminder2Period = ReminderPeriod.Hours,
                Reminder2Enabled = true,
                Reminder3Number = 2 * 60 * 24,
                Reminder3Period = ReminderPeriod.Days,
                Reminder3Enabled = true,
                Reminder4Number = 1 * 60 * 24 * 7,
                Reminder4Period = ReminderPeriod.Weeks,
                Reminder4Enabled = true
            };
            r.User = user;
            DateTime date = new DateTime(2013, 08, 10, 2, 30, 0);
            r.ReminderDate = date;

            r.HandleDelivery(HandleReminderType.Real);
            r.Reminder1Fired.ShouldBe(false);
            r.Reminder2Fired.ShouldBe(false);
            r.Reminder3Fired.ShouldBe(true);
            r.Reminder4Fired.ShouldBe(true);
        }
        [Test]
        public void handle_delivery_non_recurring_marks_as_handled()
        {
            var r = new Reminder();
            DateTime date = new DateTime(2013, 08, 10, 2, 30, 0);
            r.ReminderDate = date;
            r.HandleDelivery(HandleReminderType.Real);

            r.ReminderDate.ShouldBe(date);
            r.Handled.ShouldBe(true);
        }

        [Test]
        public void recur_weekly()
        {
            var r = new Reminder();
            r.Recurrence = Recurrence.Weekly;
            DateTime date = new DateTime(2013, 08, 10, 2, 30, 0);
            r.ReminderDate = date;

            r.HandleDelivery(HandleReminderType.Real);
            r.ReminderDate.ShouldBe(date.AddDays(7));
            r.Handled.ShouldBe(false);
        }

        [Test]
        public void recur_daily()
        {
            var r = new Reminder();
            r.Recurrence = Recurrence.Daily;
            DateTime date = new DateTime(2013, 08, 10, 2, 30, 0);
            r.ReminderDate = date;

            r.HandleDelivery(HandleReminderType.Real);
            r.ReminderDate.ShouldBe(date.AddDays(1));
            r.Handled.ShouldBe(false);
        }

        [TestCase(Recurrence.Daily, 1, 0)]
        [TestCase(Recurrence.Weekly, 7, 0)]
        //[TestCase(Recurrence.Monthly, 31)]
        [TestCase(Recurrence.Weekday, 1, 0)]
        [TestCase(Recurrence.Weekend, 7, 0)]
        [TestCase(Recurrence.Yearly, 365, 0)]
        [TestCase(Recurrence.EveryXDays, 2, 2)]
        [TestCase(Recurrence.EveryXWeeks, 14, 2)]
        [TestCase(Recurrence.EveryXMonths, 61, 2)]
        //[TestCase(Recurrence.EveryXYears, 365 * 2, 2)]
        public void recur_over_pst_boundary(Recurrence recurrence, int days, int xPeriod)
        {
            var r = new Reminder();
            r.User = new User() { TimeZone = "America/Los_Angeles" };
            r.Recurrence = recurrence;
            r.RecurrencePeriod = xPeriod;
            DateTime date = new DateTime(2014, 03, 09, 1, 30, 0);
            r.ReminderDate = date;

            r.HandleDelivery(HandleReminderType.Real);
            r.ReminderDate.ShouldBe(date.AddDays(days).AddHours(-1));
            r.Handled.ShouldBe(false);
        }

        [TestCase(Recurrence.Daily, 1, 0)]
        [TestCase(Recurrence.Weekly, 7, 0)]
        //[TestCase(Recurrence.Monthly, 31)]
        [TestCase(Recurrence.Weekday, 1, 0)]
        [TestCase(Recurrence.Weekend, 7, 0)]
        [TestCase(Recurrence.Yearly, 365, 0)]
        [TestCase(Recurrence.EveryXDays, 2, 2)]
        [TestCase(Recurrence.EveryXWeeks, 14, 2)]
        [TestCase(Recurrence.EveryXMonths, 61, 2)]
        //[TestCase(Recurrence.EveryXYears, 365 * 2, 2)]
        public void recur_over_unpst_boundary(Recurrence recurrence, int days, int xPeriod)
        {
            var r = new Reminder();
            r.User = new User() { TimeZone = "America/Los_Angeles" };
            r.Recurrence = recurrence;
            r.RecurrencePeriod = xPeriod;
            DateTime date = new DateTime(2014, 11, 2, 1, 30, 0);
            r.ReminderDate = date;

            r.HandleDelivery(HandleReminderType.Real);
            r.ReminderDate.ShouldBe(date.AddDays(days).AddHours(1));
            r.Handled.ShouldBe(false);
        }
        [Test]
        public void recur_weekday()
        {
            // First one on thursday
            var r = new Reminder();
            r.Recurrence = Recurrence.Weekday;
            DateTime date = new DateTime(2013, 09, 26, 2, 30, 0);
            r.ReminderDate = date;

            // Next one on friday
            r.HandleDelivery(HandleReminderType.Real);
            r.ReminderDate.ShouldBe(date.AddDays(1));
            r.Handled.ShouldBe(false);

            // Next one on monday
            r.HandleDelivery(HandleReminderType.Real);
            r.ReminderDate.ShouldBe(date.AddDays(4));
            r.Handled.ShouldBe(false);
        }

        [Test]
        public void recur_weekend()
        {
            // First one on saturday
            var r = new Reminder();
            r.Recurrence = Recurrence.Weekend;
            DateTime date = new DateTime(2013, 09, 28, 2, 30, 0);
            r.ReminderDate = date;

            // Next one next saturday
            r.HandleDelivery(HandleReminderType.Real);
            r.ReminderDate.ShouldBe(date.AddDays(7));
            r.Handled.ShouldBe(false);
        }

        [Test]
        public void recur_every_x_days()
        {
            // First one on saturday
            var r = new Reminder();
            r.Recurrence = Recurrence.EveryXDays;
            r.RecurrencePeriod = 11;
            DateTime date = new DateTime(2013, 09, 28, 2, 30, 0);
            r.ReminderDate = date;

            // Next one 11 days later
            r.HandleDelivery(HandleReminderType.Real);
            r.ReminderDate.ShouldBe(date.AddDays(11));
            r.Handled.ShouldBe(false);
        }

        [Test]
        public void recur_every_x_weeks()
        {
            // First one on saturday
            var r = new Reminder();
            r.Recurrence = Recurrence.EveryXWeeks;
            r.RecurrencePeriod = 3;
            DateTime date = new DateTime(2013, 09, 28, 2, 30, 0);
            r.ReminderDate = date;

            // Next one 3 weeks later
            r.HandleDelivery(HandleReminderType.Real);
            r.ReminderDate.ShouldBe(date.AddDays(21));
            r.Handled.ShouldBe(false);
        }

        [Test]
        public void recur_every_x_months()
        {
            // First one on saturday
            var r = new Reminder();
            r.Recurrence = Recurrence.EveryXMonths;
            r.RecurrencePeriod = 2;
            DateTime date = new DateTime(2013, 09, 28, 2, 30, 0);
            r.ReminderDate = date;

            // Next one 2 months later
            r.HandleDelivery(HandleReminderType.Real);
            r.ReminderDate.ShouldBe(date.AddMonths(2));
            r.Handled.ShouldBe(false);
        }

        [Test]
        public void recur_every_x_years()
        {
            // First one on saturday
            var r = new Reminder();
            r.Recurrence = Recurrence.EveryXYears;
            r.RecurrencePeriod = 2;
            DateTime date = new DateTime(2013, 09, 28, 2, 30, 0);
            r.ReminderDate = date;

            // Next one 2 years later
            r.HandleDelivery(HandleReminderType.Real);
            r.ReminderDate.ShouldBe(date.AddYears(2));
            r.Handled.ShouldBe(false);
        }

        [Test]
        public void recur_yearly()
        {
            var r = new Reminder();
            r.Recurrence = Recurrence.Yearly;
            DateTime date = new DateTime(2013, 08, 10, 2, 30, 0);
            r.ReminderDate = date;

            r.HandleDelivery(HandleReminderType.Real);
            r.ReminderDate.ShouldBe(date.AddYears(1));
            r.Handled.ShouldBe(false);
        }

        [Test]
        public void gettimefor_days()
        {
            var r = new Reminder();
            r.User = new User();
            r.User.Reminder1Period = ReminderPeriod.Days;
            r.User.Reminder1Number = 3 * 24 * 60;
            r.GetTimeFor(HandleReminderType.One).ShouldBe("3 days");

            r.User.Reminder2Period = ReminderPeriod.Minutes;
            r.User.Reminder2Number = 1;
            r.GetTimeFor(HandleReminderType.Two).ShouldBe("1 minute");

            r.User.Reminder3Period = ReminderPeriod.Hours;
            r.User.Reminder3Number = 77 * 60;
            r.GetTimeFor(HandleReminderType.Three).ShouldBe("77 hours");

            r.User.Reminder4Period = ReminderPeriod.Weeks;
            r.User.Reminder4Number = 11 * 7 * 24 * 60;
            r.GetTimeFor(HandleReminderType.Four).ShouldBe("11 weeks");
        }

        [Test]
        public void gettimefor_real_reminder_returns_nothing()
        {
            var r = new Reminder();
            r.GetTimeFor(HandleReminderType.Real).ShouldBe("");
        }
    }
}
