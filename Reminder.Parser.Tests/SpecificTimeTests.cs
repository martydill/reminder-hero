using System;
using NUnit.Framework;
using ReminderHero.Parser;
using Shouldly;

namespace ReminderHero.Data.Tests
{
    public class SpecificTimeTests
    {
        [TestCase("2pm")]
        [TestCase("2:00 p.m.")]
        [TestCase("2 p.m.")]
        [TestCase("2p.m.")]
        [TestCase("2:00PM.")]
        [TestCase("2:00PM")]
        [TestCase("2 pm")]
        [TestCase("2:00 pm")]
        [TestCase("14:00")]
        [TestCase("2.00pm")]
        [TestCase("2.00 pm")]
        [TestCase("14.00")]
        [TestCase("14.00 pm")]
        [TestCase("200pm")]
        public void exact_time_2pm(string time)
        {
            var result = ReminderParser.Parse("do stuff today at " + time, new DateTime(2012, 04, 04, 11, 0, 0));
            result.Description.ShouldBe("do stuff");
            result.ReminderDate.ShouldBeGreaterThan(result.CreatedDate);
            result.ReminderDate.Hour.ShouldBe(14);
        }

        [Test]
        public void tenpm()
        {
            var result = ReminderParser.Parse("remind me to do stuff at 10pm", DateTime.Now);
            result.Description.ShouldBe("to do stuff");
            result.ReminderDate.Hour.ShouldBe(22);
        }

        [Test]
        public void handles_times_without_colons()
        {
            var result = ReminderParser.Parse("call dad at 430 PM today", DateTime.Now);
            result.ReminderDate.Hour.ShouldBe(16);
            result.ReminderDate.Minute.ShouldBe(30);
        }

        [Test]
        public void time_and_tomorrow()
        {
            var now = new DateTime(2013, 08, 20, 11, 0, 0);
            var result = ReminderParser.Parse("remind me to do stuff tomorrow at 2:30 PM", now);
            result.Description.ShouldBe("to do stuff");
            result.ReminderDate.Hour.ShouldBe(14);
            result.ReminderDate.Minute.ShouldBe(30);
            result.ReminderDate.Day.ShouldBe(21);
        }

        [Test]
        public void time_and_day_of_week()
        {
            var now = new DateTime(2013, 08, 31, 11, 0, 0);
            var result = ReminderParser.Parse("do stuff wednesday at 10pm", now);
            result.Description.ShouldBe("do stuff");
            result.ReminderDate.Month.ShouldBe(9);
            result.ReminderDate.Day.ShouldBe(4);
            result.ReminderDate.Hour.ShouldBe(22);
        }

        [Test]
        public void asdf()
        {
            var now = new DateTime(2013, 09, 01, 12, 35, 0);
            var result = ReminderParser.Parse("take the girls OS at 12:45pm ", now);
            result.Description.ShouldBe("take the girls OS");
            result.ReminderDate.Month.ShouldBe(9);
            result.ReminderDate.Day.ShouldBe(1);
            result.ReminderDate.Hour.ShouldBe(12);
        }

        [Test]
        public void extra_on()
        {
            var now = new DateTime(2013, 09, 01, 12, 35, 0);
            var result = ReminderParser.Parse("take out the trash on monday at 9:30am  ", now);
            result.Description.ShouldBe("take out the trash");
            result.ReminderDate.Month.ShouldBe(9);
            result.ReminderDate.Day.ShouldBe(2);
            result.ReminderDate.Hour.ShouldBe(9);
        }
    }
}