using System;
using NUnit.Framework;
using ReminderHero.Models;
using Shouldly;

namespace ReminderHero.Data.Tests
{
    public class ReminderSettingTests 
    {
        private Reminder _reminder;

        [SetUp]
        public void Setup()
        {
            var user = new User();
            user.Reminder1Enabled = true;
            user.Reminder1Number = 15;
            user.Reminder1Period = ReminderPeriod.Minutes;
            user.TimeZone = "America/Los_Angeles";
            _reminder = new Reminder();
            _reminder.User = user;

            var date = new DateTime(2013, 08, 10, 2, 30, 0);
            _reminder.ReminderDate = date;
        }

        [Test]
        public void handle_reminder_1_works()
        {
            _reminder.HandleDelivery(HandleReminderType.One);
            _reminder.Handled.ShouldBe(false);
            _reminder.Reminder1Fired.ShouldBe(true);
        }

        [Test]
        public void handle_reminder_2_works()
        {
            _reminder.HandleDelivery(HandleReminderType.Two);
            _reminder.Handled.ShouldBe(false);
            _reminder.Reminder2Fired.ShouldBe(true);
        }

        [Test]
        public void handle_reminder_3_works()
        {
            _reminder.HandleDelivery(HandleReminderType.Three);
            _reminder.Handled.ShouldBe(false);
            _reminder.Reminder3Fired.ShouldBe(true);
        }

        [Test]
        public void handle_reminder_4_works()
        {
            _reminder.HandleDelivery(HandleReminderType.Four);
            _reminder.Handled.ShouldBe(false);
            _reminder.Reminder4Fired.ShouldBe(true);
        }
    }
}
