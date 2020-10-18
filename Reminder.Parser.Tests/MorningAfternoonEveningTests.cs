using System;
using NUnit.Framework;
using ReminderHero.Parser;
using Shouldly;

namespace ReminderHero.Data.Tests
{
    public class MorningAfternoonEveningTests
    {
        [TestCase("take out the trash tomorrow morning", ReminderParser.MorningHour)]
        [TestCase("take out the trash tomorrow afternoon", ReminderParser.AfternoonHour)]
        [TestCase("take out the trash tomorrow evening", ReminderParser.EveningHour)]
        [TestCase("take out the trash tomorrow night", ReminderParser.NightHour)]
        [TestCase("take out the trash tomorrow at noon", ReminderParser.NoonHour)]
        public void tomorrow_test(string text, int hours)
        {
            var result = ReminderParser.Parse(text, DateTime.Now);
            result.Description.ShouldBe("take out the trash");
            var expectedDate = new DateTime(result.CreatedDate.Year, result.CreatedDate.Month, result.CreatedDate.Day)
                .AddDays(1)
                .AddHours(hours);

            result.ReminderDate.ShouldBe(expectedDate);
        }

        [TestCase("take out the trash this evening", ReminderParser.EveningHour)]
        [TestCase("take out the trash this afternoon", ReminderParser.AfternoonHour)]
        [TestCase("take out the trash tonight", ReminderParser.NightHour)]
        [TestCase("take out the trash this morning", ReminderParser.MorningHour)]
        [TestCase("take out the trash today at noon", ReminderParser.NoonHour)]
        public void today_test(string text, int hours)
        {
            var result = ReminderParser.Parse(text, DateTime.Now);
            result.Description.ShouldBe("take out the trash");
            var expectedDate = new DateTime(result.CreatedDate.Year, result.CreatedDate.Month, result.CreatedDate.Day)
                .AddHours(hours);

            result.ReminderDate.ShouldBe(expectedDate);
        }

        [TestCase("take out the trash monday evening", ReminderParser.EveningHour, DayOfWeek.Monday)]
        [TestCase("take out the trash tuesday afternoon", ReminderParser.AfternoonHour, DayOfWeek.Tuesday)]
        [TestCase("take out the trash wednesday night", ReminderParser.NightHour, DayOfWeek.Wednesday)]
        [TestCase("take out the trash thursday morning", ReminderParser.MorningHour, DayOfWeek.Thursday)]
        [TestCase("take out the trash friday morning", ReminderParser.MorningHour, DayOfWeek.Friday)]
        [TestCase("take out the trash saturday at noon", ReminderParser.NoonHour, DayOfWeek.Saturday)]
        [TestCase("take out the trash sunday evening", ReminderParser.EveningHour, DayOfWeek.Sunday)]
        public void weekday_test(string text, int hours, DayOfWeek day)
        {
            var result = ReminderParser.Parse(text, DateTime.Now);
            result.Description.ShouldBe("take out the trash");

            result.ReminderDate.DayOfWeek.ShouldBe(day);
            result.ReminderDate.Hour.ShouldBe(hours);
            result.ReminderDate.ShouldBeGreaterThan(result.CreatedDate);
        }

        [Ignore]
        [TestCase("morning", 6, 6)]
        [TestCase("evening", 6, 18)]
        [TestCase("night", 6, 18)]
        public void time_with_relative_period_but_no_ampm(string word, int hour, int expectedReminderHour)
        {
            var result = ReminderParser.Parse("Grab a donut at " + hour + ":30 tomorrow " + word + "    Sincerely, Joe Smith", DateTime.Now);
            result.ReminderDate.Hour.ShouldBe(expectedReminderHour);
            result.ReminderDate.Day.ShouldBe(DateTime.Now.AddDays(1).Day);
            result.Description.ShouldBe("Grab a donut");
        }

        [Ignore]
        [TestCase("morning")]
        [TestCase("evening")]
        [TestCase("night")]
        public void time_with_relative_period_and_ampm_ignores_period(string word)
        {
            var result = ReminderParser.Parse("Grab a donut at 6:30 am tomorrow " + word + "    Sincerely, Joe Smith", DateTime.Now);
            result.ReminderDate.Hour.ShouldBe(6);
            result.ReminderDate.Minute.ShouldBe(30);
            result.Description.ShouldBe("Grab a donut");
        }
    }
}