using System;
using NUnit.Framework;
using ReminderHero.Models;
using ReminderHero.Parser;
using Shouldly;

namespace ReminderHero.Data.Tests
{
    public class RemindersWithoutAtTests
    {
        [TestCase("4pm Nov 5 remind me eat dinner")]
        [TestCase("Nov 5 4pm eat dinner")]
        [TestCase("eat dinner 4pm Nov 5")]
        [TestCase("eat dinner Nov 5 4pm")]
        public void date_and_time(string text)
        {
            var now = new DateTime(2013, 08, 08);
            var result = ReminderParser.Parse(text, now);
            result.ReminderDate.Day.ShouldBe(5);
            result.ReminderDate.Month.ShouldBe(11);
            result.ReminderDate.Hour.ShouldBe(16);
            result.Description.ShouldBe("eat dinner");
        }

        [TestCase("4pm remind me eat dinner")]
        [TestCase("4pm eat dinner")]
        [TestCase("eat dinner 4pm")]
        public void time_only(string text)
        {
            var now = new DateTime(2013, 08, 08);
            var result = ReminderParser.Parse(text, now);
            result.ReminderDate.Day.ShouldBe(8);
            result.ReminderDate.Hour.ShouldBe(16);
            result.Description.ShouldBe("eat dinner");
        }

        [TestCase("20m eat dinner")]
        [TestCase("eat dinner 20m")]
        public void time_offset_only(string text)
        {
            var result = ReminderParser.Parse(text, DateTime.Now);
            result.ReminderDate.ShouldBe(result.CreatedDate.AddMinutes(20));
            result.Description.ShouldBe("eat dinner");
        }

        [Test]
        public void real_reminder_1()
        {
            var now = new DateTime(2013, 08, 08);
            var v = ReminderParser.Parse("November 5th 12:00 remind me of the thing.  ", now); 
            v.Description.ShouldBe("of the thing.");
            v.ReminderDate.ShouldBe(new DateTime(2013, 11, 5, 12, 0, 0));
        }

        [Test]
        public void real_reminder_2()
        {
            var now = new DateTime(2013, 08, 08);
            var v = ReminderParser.Parse("15.30 aaa bbb ccc", now);
            v.Description.ShouldBe("aaa bbb ccc");
            v.ReminderDate.ShouldBe(new DateTime(2013, 08, 08, 15, 30, 0));
        }
    }
}
