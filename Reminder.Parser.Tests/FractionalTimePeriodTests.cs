using System;
using NUnit.Framework;
using ReminderHero.Parser;
using Shouldly;

namespace ReminderHero.Data.Tests
{
    public class FractionalTimePeriodTests
    {
        [TestCase(".")]
        [TestCase(",")]
        public void handles_xpointy_minutes(string separator)
        {
            
            var result = ReminderParser.Parse("do important stuff in 3" + separator + "5 minutes", DateTime.Now);
            result.ReminderDate.ShouldBe(result.CreatedDate.AddSeconds(210));
        }

        [TestCase(".")]
        [TestCase(",")]
        public void handles_xpointy_hours(string separator)
        {
            var result = ReminderParser.Parse("do important stuff in " + separator + "5 hours", DateTime.Now);
            result.ReminderDate.ShouldBe(result.CreatedDate.AddMinutes(210));
        }

        [TestCase(".")]
        [TestCase(",")]
        public void handles_xpointy_days(string separator)
        {
            var result = ReminderParser.Parse("do important stuff in " + separator + "5 days", DateTime.Now);
            result.ReminderDate.ShouldBe(result.CreatedDate.AddHours(84));
        }

        [TestCase(".")]
        [TestCase(",")]
        public void handles_xpointy_weeks(string separator)
        {
            var result = ReminderParser.Parse("do important stuff in " + separator + "5 weeks", DateTime.Now);
            result.ReminderDate.ShouldBe(result.CreatedDate.AddHours(3 * 24 * 7 + (12 * 7)));
        }
    }
}