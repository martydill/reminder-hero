using System;
using NUnit.Framework;
using ReminderHero.Parser;
using Shouldly;

namespace ReminderHero.Data.Tests
{
    [TestFixture]
    class NextWeekTests
    {
        [Test]
        public void next_week_doesnt_apply_if_its_part_of_valid_reminder()
        {
            var date = new DateTime(2013, 11, 8, 11, 0, 0);
            var r = ReminderParser.Parse("in 2h remind me to call Jim next week", date);
            r.ReminderDate.Day.ShouldBe(8);
            r.ReminderDate.Hour.ShouldBe(13);
            r.Description.ShouldBe("to call Jim next week");
        }

        [Test]
        public void next_week_from_monday_goes_to_next_monday()
        {
            var date = new DateTime(2013, 11, 11);
            var r = ReminderParser.Parse("call jim next week", date);
            r.ReminderDate.Day.ShouldBe(18);
        }

        [Test]
        public void next_week_from_friday_goes_to_monday()
        {
             var date = new DateTime(2013, 11, 8);
            var r = ReminderParser.Parse("call jim next week", date);
            r.ReminderDate.Day.ShouldBe(11);
        }

        [Test]
        public void next_week_from_sunday_goes_to_next_sunday()
        {
            var date = new DateTime(2013, 11, 10);
            var r = ReminderParser.Parse("call jim next week", date);
            r.ReminderDate.Day.ShouldBe(17);
        }

        [Test]
        public void next_week_handles_stuff_before_and_after()
        {
            var date = new DateTime(2013, 11, 8);
            var r = ReminderParser.Parse("call Jim next week and tell him he's cool    -- ", date);
            r.ReminderDate.Day.ShouldBe(11);
            r.Description.ShouldBe("call Jim and tell him he's cool");
        }
    }

    [TestFixture]
    class NextMonthTests
    {
        [Test]
        public void next_month_doesnt_apply_if_its_part_of_valid_reminder()
        {
            var date = new DateTime(2013, 11, 8, 11, 0, 0);
            var r = ReminderParser.Parse("in 2h remind me to call Jim next month", date);
            r.ReminderDate.Day.ShouldBe(8);
            r.ReminderDate.Hour.ShouldBe(13);
            r.Description.ShouldBe("to call Jim next month");
        }

        [Test]
        public void next_month_from_first_goes_to_first()
        {
            var date = new DateTime(2013, 11, 1);
            var r = ReminderParser.Parse("call jim next month", date);
            r.ReminderDate.Day.ShouldBe(1);
            r.ReminderDate.Month.ShouldBe(12);
        }

        [Test]
        public void next_month_from_30th_goes_to_first()
        {
            var date = new DateTime(2013, 11, 30);
            var r = ReminderParser.Parse("call jim next month", date);
            r.ReminderDate.Day.ShouldBe(1);
            r.ReminderDate.Month.ShouldBe(12);
            r.Description.ShouldBe("call jim");
        }

        [Test]
        public void next_month_handles_stuff_before_and_after()
        {
            var date = new DateTime(2013, 11, 8);
            var r = ReminderParser.Parse("call Jim next month and tell him he's cool    -- ", date);
            r.ReminderDate.Day.ShouldBe(1);
            r.ReminderDate.Month.ShouldBe(12);
            r.Description.ShouldBe("call Jim and tell him he's cool");
        }
    }

    [TestFixture]
    class NextYearTests
    {
        [Test]
        public void next_year_doesnt_apply_if_its_part_of_valid_reminder()
        {
            var date = new DateTime(2013, 11, 8, 11, 0, 0);
            var r = ReminderParser.Parse("in 2h remind me to call Jim next year", date);
            r.ReminderDate.Day.ShouldBe(8);
            r.ReminderDate.Hour.ShouldBe(13);
            r.Description.ShouldBe("to call Jim next year");
        }

        [Test]
        public void next_year_from_first_goes_to_first()
        {
            var date = new DateTime(2013, 12, 31);
            var r = ReminderParser.Parse("call jim next year", date);
            r.ReminderDate.Day.ShouldBe(1);
            r.ReminderDate.Month.ShouldBe(1);
            r.ReminderDate.Year.ShouldBe(2014);
        }

        [Test]
        public void next_year_from_middle_of_year_goes_to_first()
        {
            var date = new DateTime(2013, 5, 14);
            var r = ReminderParser.Parse("call jim next year", date);
            r.ReminderDate.Day.ShouldBe(1);
            r.ReminderDate.Month.ShouldBe(1);
            r.ReminderDate.Year.ShouldBe(2014);
            r.Description.ShouldBe("call jim");
        }

        [Test]
        public void next_year_handles_stuff_before_and_after()
        {
            var date = new DateTime(2013, 11, 8);
            var r = ReminderParser.Parse("call Jim next year and tell him he's cool    -- ", date);
            r.ReminderDate.Day.ShouldBe(1);
            r.ReminderDate.Month.ShouldBe(1);
            r.ReminderDate.Year.ShouldBe(2014);
            r.Description.ShouldBe("call Jim and tell him he's cool");
        }
    }
}
