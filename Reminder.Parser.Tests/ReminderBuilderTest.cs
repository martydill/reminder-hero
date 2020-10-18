
using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using ReminderHero.Models;
using ReminderHero.Models.DataAccess;
using ReminderHero.Parser;
using Shouldly;

namespace ReminderHero.Data.Tests
{
    [TestFixture]
    public class ReminderBuilderTest
    {
        private Mock<IReminderRepository> _repo;

        [SetUp]
        public void Setup()
        {
            _repo = new Mock<IReminderRepository>();
        }

        [Test]
        public void build_shorter_time_than_reminder_settings_marks_as_handled()
        {
            var date = new DateTime(2013, 08, 07, 9, 0, 0);
            var user = new User()
            {
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

            var result = ReminderBuilder.Build("a@a.net", "", "do stuff at 10am", date, new Endpoint(), user, _repo.Object);
            result.Reminder1Fired.ShouldBe(true);
            result.Reminder2Fired.ShouldBe(true);
            result.Reminder3Fired.ShouldBe(true);
            result.Reminder4Fired.ShouldBe(true);
        }

        [Test]
        public void marty_shouldnt_have_gotten_a_3_hour_reminder_for_this()
        {
            var date = new DateTime(2014, 05, 15, 8, 7, 0);
            var user = new User()
            {
                Reminder1Number = 15,
                Reminder1Period = ReminderPeriod.Minutes,
                Reminder1Enabled = true,
                Reminder2Number = 3 * 60,
                Reminder2Period = ReminderPeriod.Hours,
                Reminder2Enabled = true,
                Reminder3Number = 2 * 60 * 24,
                Reminder3Period = ReminderPeriod.Days,
                Reminder3Enabled = true,
                Reminder4Number = 1 * 60 * 24 * 7,
                Reminder4Period = ReminderPeriod.Weeks,
                Reminder4Enabled = true
            };

            var result = ReminderBuilder.Build("a@a.net", "", "really important things to do in 20 minutes", date, new Endpoint(), user, _repo.Object);
            result.Reminder1Fired.ShouldBe(false);
            result.Reminder2Fired.ShouldBe(true);
            result.Reminder3Fired.ShouldBe(true);
            result.Reminder4Fired.ShouldBe(true);
        }

        [Test]
        public void build_longer_time_than_reminder_settings_doesnt_mark_as_handled()
        {
            var date = new DateTime(2013, 08, 07, 9, 0, 0);
            var user = new User()
            {
                Reminder1Number = 100,
                Reminder1Period = ReminderPeriod.Minutes,
                Reminder1Enabled = true,
                Reminder2Number = 1 * 60,
                Reminder2Period = ReminderPeriod.Hours,
                Reminder2Enabled = true,
                Reminder3Number = 1 * 60 * 24,
                Reminder3Period = ReminderPeriod.Days,
                Reminder3Enabled = true,
                Reminder4Number = 1 * 60 * 24 * 7,
                Reminder4Period = ReminderPeriod.Weeks,
                Reminder4Enabled = true
            };

            var result = ReminderBuilder.Build("a@a.net", "", "do stuff in 999 hours", date, new Endpoint(), user, _repo.Object);
            result.Reminder1Fired.ShouldBe(false);
            result.Reminder2Fired.ShouldBe(false);
            result.Reminder3Fired.ShouldBe(false);
            result.Reminder4Fired.ShouldBe(false);
        }

        [Test]
        public void build_pst()
        {
            var date = new DateTime(2013, 08, 07, 9, 0, 0);
            var result = ReminderBuilder.Build("a@a.net", "", "do stuff at 10pm", date, new Endpoint(), null, _repo.Object);
            result.ReminderDate.Hour.ShouldBe(5);
            result.ReminderDate.Day.ShouldBe(8);
        }

        //[Test]
        //public void build_est()
        //{
        //    var date = new DateTime(2013, 08, 07, 9, 0, 0);
        //    var result = ReminderBuilder.Build("a@a.net", "", "do stuff at 10pm", "Sun, 25 Aug 2013 21:07:20 -0400", date, null, null);
        //    result.ReminderDate.Hour.ShouldBe(2);
        //    result.ReminderDate.Day.ShouldBe(8);
        //}

        //[Test]j
        //public void build_est_relative_time()
        //{
        //    var date = new DateTime(2013, 08, 07, 9, 0, 0);
        //    var result = ReminderBuilder.Build("a@a.net", "", "do stuff in 5 min", "Sun, 25 Aug 2013 21:07:20 -0400", date, null, null);
        //    result.ReminderDate.ToLocalTime().Hour.ShouldBe(9);
        //    result.ReminderDate.ToLocalTime().Day.ShouldBe(7);
        //    result.ReminderDate.ToLocalTime().Minute.ShouldBe(5);
        //}

        //[Test]
        //public void build_london()
        //{
        //    var date = new DateTime(2013, 08, 07, 9, 0, 0);
        //    var result = ReminderBuilder.Build("a@a.net", "", "do stuff at 10pm", "Sun, 25 Aug 2013 21:07:20 +0100", date, null, null);
        //    result.ReminderDate.Hour.ShouldBe(21);
        //    result.ReminderDate.Day.ShouldBe(7);
        //}

        //[Test]
        //public void utc_conversion_works()
        //{
        //    var now = new DateTime(2013, 09, 01, 12, 35, 0);
        //    var result = ReminderBuilder.Build("a@a.net", "", "take the girls OS at 12:45pm ", "Date: Sun, 1 Sep 2013 12:35:49 -0700", now, null, null);
        //    result.ReminderDate.Hour.ShouldBe(19);
        //    result.ReminderDate.Day.ShouldBe(01);
        //}

        //[Test]
        //public void uses_endpoint_if_it_exists()
        //{
        //    var g = Guid.NewGuid();
        //    var endpoint = new Endpoint() {Address = "a@a.net", UtcOffset = -7, UserId = g};
        //    var now = new DateTime(2013, 09, 01, 12, 35, 0);
        //    var result = ReminderBuilder.Build("a@a.net", "", "take the girls OS at 12:45pm ", "Date: Sun, 1 Sep 2013 19:35:49 +0000", now, endpoint, null);
        //    result.ReminderDate.Hour.ShouldBe(19);
        //    result.ReminderDate.Day.ShouldBe(01);
        //    result.UserId.ShouldBe(g);
        //}

        [Test]
        public void uses_subject_if_set()
        {
            var now = new DateTime(2013, 09, 01, 12, 35, 0);
            var result = ReminderBuilder.Build("a@a.net", "eat in 20 mins", "Cheers, Bob", now, new Endpoint(), null, _repo.Object);
            result.ReminderDate.Hour.ShouldBe(19);
            result.ReminderDate.Day.ShouldBe(1);
            result.ReminderDate.Minute.ShouldBe(55);
        }

        [Test]
        public void uses_message_if_subject_set_but_not_a_reminder()
        {
            var now = new DateTime(2013, 09, 01, 12, 35, 0);
            var result = ReminderBuilder.Build("a@a.net", "pvst", "Get ready in 2 hours    --   Sent using mobile....  ", now, new Endpoint(), null, _repo.Object);
            result.ReminderDate.Hour.ShouldBe(21);
            result.ReminderDate.Day.ShouldBe(1);
            result.ReminderDate.Minute.ShouldBe(35);
        }

        [TestCase("America/Los_Angeles", -7, 0)]
        [TestCase("America/Chicago", -5, 0)]
        [TestCase("America/New_York", -4, 0)]
        [TestCase("Europe/Amsterdam", 2, 0)]
        [TestCase("Asia/Hong_Kong", 8, 0)]
        [TestCase("Australia/Adelaide", 9, 30)]
        public void timezone_day_of_week_tests(string tzName, int utcOffset, int minuteOffset)
        {
            var user = new User() { TimeZone = tzName };
            var now = new DateTime(2013, 10, 20, 10, 45, 0);
            var result = ReminderBuilder.Build("a@a.net", "", "do stuff on Tuesday at 12:45 pm",
                                                now, new Endpoint(), user, _repo.Object);
            result.ReminderDate.Day.ShouldBe(22);
            result.ReminderDate.Hour.ShouldBe(12 - utcOffset - (45 + minuteOffset) / 60);
            result.ReminderDate.Minute.ShouldBe((45 + minuteOffset) % 60);
        }

        [TestCase("America/Los_Angeles", -7, 0)]
        [TestCase("America/Chicago", -5, 0)]
        [TestCase("America/New_York", -4, 0)]
        [TestCase("Europe/Amsterdam", 2, 0)]
        [TestCase("Asia/Hong_Kong", 8, 0)]
        [TestCase("Australia/Adelaide", 10, 30)]
        public void timezone_time_tests(string tzName, int utcOffset, int minuteOffset)
        {
            var user = new User() { TimeZone = tzName };
            var now = new DateTime(2013, 10, 20, 10, 45, 0);
            var result = ReminderBuilder.Build("a@a.net", "", "do stuff at 5:22 pm",
                                                now, new Endpoint(), user, _repo.Object);
            var dayOffset = 0;
            var hourOffset = (17 - utcOffset);
            if (minuteOffset == 30)
                hourOffset -= 1;//- (22 + minuteOffset) / 60);
            if (hourOffset >= 24)
            {
                dayOffset = 1;
                hourOffset -= 24;
            }
            if (utcOffset > 4)///  not sure if right...
                dayOffset += 1;

            result.ReminderDate.Day.ShouldBe(20 + dayOffset);
            result.ReminderDate.Hour.ShouldBe(hourOffset);
            result.ReminderDate.Minute.ShouldBe((22 + minuteOffset) % 60);
        }

        [TestCase("America/Los_Angeles", -8, 0)]
        [TestCase("America/Chicago", -6, 0)]
        [TestCase("America/New_York", -5, 0)]
        [TestCase("Europe/Amsterdam", 1, 0)]
        [TestCase("Asia/Hong_Kong", 8, 0)]
        [TestCase("Australia/Adelaide", 10, 30)]
        public void timezone_specific_day_tests_dst_overlap(string tzName, int utcOffset, int minuteOffset)
        {
            var user = new User() { TimeZone = tzName };
            var now = new DateTime(2013, 10, 20, 10, 45, 0);
            var result = ReminderBuilder.Build("a@a.net", "", "do stuff on Nov 10 at 7:20 am",
                                                now, new Endpoint(), user, _repo.Object);
            result.ReminderDate.Month.ShouldBe(11);

            var desiredHour = 7 - utcOffset - (20 + minuteOffset) / 60;

            result.ReminderDate.Day.ShouldBe(10 + (desiredHour < 0 ? -1 : 0));
            if (desiredHour == -1) desiredHour = 0;
            if (desiredHour > 0)
                result.ReminderDate.Hour.ShouldBe(desiredHour);

            else
                result.ReminderDate.Hour.ShouldBe(23 + desiredHour);
            result.ReminderDate.Minute.ShouldBe((20 + minuteOffset) % 60);
        }

        [TestCase("America/Los_Angeles")]
        [TestCase("America/Chicago")]
        [TestCase("America/New_York")]
        [TestCase("Europe/Amsterdam")]
        [TestCase("Asia/Hong_Kong")]
        [TestCase("Australia/Adelaide")]
        public void relative_time_tests_should_all_have_same_utc_time_hours(string tzName)
        {
            var user = new User() { TimeZone = tzName };
            var now = new DateTime(2013, 10, 20, 10, 45, 0);
            var result = ReminderBuilder.Build("a@a.net", "", "do stuff in 5h",
                                                now, new Endpoint(), user, _repo.Object);
            result.ReminderDate.Day.ShouldBe(20);
            result.ReminderDate.Hour.ShouldBe(22);
            result.ReminderDate.Minute.ShouldBe(45);
        }

        [TestCase("America/Los_Angeles")]
        [TestCase("America/Chicago")]
        [TestCase("America/New_York")]
        [TestCase("Europe/Amsterdam")]
        [TestCase("Asia/Hong_Kong")]
        [TestCase("Australia/Adelaide")]
        public void relative_time_tests_should_all_have_same_utc_time_days(string tzName)
        {
            var user = new User() { TimeZone = tzName };
            var now = new DateTime(2013, 10, 20, 14, 22, 0);
            var result = ReminderBuilder.Build("a@a.net", "", "do stuff in 2d",
                                               now, new Endpoint(), user, _repo.Object);
            result.ReminderDate.Day.ShouldBe(22);
            result.ReminderDate.Hour.ShouldBe(21);
            result.ReminderDate.Minute.ShouldBe(22);
        }
        
        [Test]
        public void over_email_reminder_limit_returns_error()
        {
            var user = new User() { PricePlanId = PricePlan.FreePlanId };
            var now = new DateTime(2013, 10, 20, 14, 22, 0);

            var fiveReminders = new List<Reminder>();
            fiveReminders.AddRange(Enumerable.Range(0, PricePlan.FreePlanEmails).Select(s => new Reminder()));

            _repo.Setup(r => r.RemindersForCurrentMonth(user, EndpointType.Email, It.IsAny<DateTime>())).Returns(fiveReminders);

            Assert.That(() =>
                        ReminderBuilder.Build("a@a.net", "", "do stuff every day at 2pm", now, new Endpoint(), user, _repo.Object),
                        Throws.TypeOf<ReminderFailureException>()
                              .And.Property("Data0").EqualTo(ReminderFailureReason.OverReminderLimit));
        }


        [Test]
        public void over_sms_reminder_limit_returns_error()
        {
            var user = new User() {PricePlanId = PricePlan.PremiumPlanId};
            var now = new DateTime(2013, 10, 20, 14, 22, 0);

            var fiveReminders = new List<Reminder>();
            fiveReminders.AddRange(Enumerable.Range(0, PricePlan.PremiumPlanSmses).Select(s => new Reminder() { ReminderType = EndpointType.Phone }));

            _repo.Setup(r => r.RemindersForCurrentMonth(user, EndpointType.Phone, It.IsAny<DateTime>())).Returns(fiveReminders);

            Assert.That(() =>
                        ReminderBuilder.Build("a@a.net", "", "do stuff every day at 2pm", now, new Endpoint() { EndpointType = EndpointType.Phone }, user, _repo.Object),
                        Throws.TypeOf<ReminderFailureException>()
                              .And.Property("Data0").EqualTo(ReminderFailureReason.OverReminderLimit));
        }

        [Test]
        public void recurring_reminder_on_free_plan_returns_error()
        {
            var user = new User() {PricePlanId = PricePlan.FreePlanId};
            var now = new DateTime(2013, 10, 20, 14, 22, 0);
            var e = new ReminderFailureException();
            
            Assert.That(() =>
                ReminderBuilder.Build("a@a.net", "", "do stuff every day at 2pm", now, new Endpoint(), user, _repo.Object),
                    Throws.TypeOf<ReminderFailureException>()
                    .And.Property("Data0").EqualTo(ReminderFailureReason.RecurringRemindersNotSupported));
        }
        
        [Test]
        public void sms_on_free_plan_returns_error()
        {
            var user = new User() { PricePlanId = PricePlan.FreePlanId };
            var now = new DateTime(2013, 10, 20, 14, 22, 0);
            var endpoint = new Endpoint() {EndpointType = EndpointType.Phone};

            Assert.That(() =>
                        ReminderBuilder.Build("1234", "", "do abc at 2pm",
                                               now, endpoint, user, _repo.Object),
                        Throws.TypeOf<ReminderFailureException>()
                              .And.Property("Data0").EqualTo(ReminderFailureReason.SmsNotSupported));
        }

        [Test]
        public void no_endpoint_returns_error()
        {
            var user = new User() { PricePlanId = PricePlan.FreePlanId };
            var now = new DateTime(2013, 10, 20, 14, 22, 0);

            Assert.That(() =>
                        ReminderBuilder.Build("1234", "", "do abc at 2pm",
                                               now, null, user, _repo.Object),
                        Throws.TypeOf<ReminderFailureException>()
                              .And.Property("Data0").EqualTo(ReminderFailureReason.UnknownEndpoint));
        }
    }
}
