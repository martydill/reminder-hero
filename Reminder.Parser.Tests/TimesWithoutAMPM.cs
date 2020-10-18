using System;
using NUnit.Framework;
using ReminderHero.Parser;
using Shouldly;

namespace ReminderHero.Data.Tests
{
    public class TimesWithoutAMPM
    {
        [Test]
        public void tomorrow_afternoon_is_pm()
        {
            var result = ReminderParser.Parse("do stuff at 3:30 tomorrow", new DateTime(2014, 04, 17, 2, 22, 0));
            result.Description.ShouldBe("do stuff");
            result.ReminderDate.ShouldBe(new DateTime(2014, 04, 18, 15, 30, 0));
        }

        [Test]
        public void tomorrow_night_is_pm()
        {
            var result = ReminderParser.Parse("do stuff at 6:30 tomorrow", new DateTime(2014, 04, 17, 15, 22, 0));
            result.Description.ShouldBe("do stuff");
            result.ReminderDate.ShouldBe(new DateTime(2014, 04, 18, 18, 30, 0));
        }

        [TestCase("6:25 pm")]
        [TestCase("6:25")] 
        [TestCase("18:25")]
        public void no_ampm_while_in_pm(string time)
        {
            var result = ReminderParser.Parse("do stuff at " + time, new DateTime(2014, 04, 17, 18, 22, 0));
            result.Description.ShouldBe("do stuff");
            result.ReminderDate.ShouldBe(new DateTime(2014, 04, 17, 18, 25, 0));
        }

        [TestCase("8:25 am")]
        [TestCase("8:25")]
        [TestCase("08:25")]
        public void no_ampm_while_in_am(string time)
        {
            var result = ReminderParser.Parse("do stuff at " + time, new DateTime(2014, 04, 17, 7, 22, 0));
            result.Description.ShouldBe("do stuff");
            result.ReminderDate.ShouldBe(new DateTime(2014, 04, 17, 8, 25, 0));
        }
    }
}