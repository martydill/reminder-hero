using System;
using NUnit.Framework;
using ReminderHero.Parser;
using Shouldly;

namespace ReminderHero.Data.Tests
{
    public class SpecificDayTests
    {
        [TestCase("jan", 1, 2014)]
        [TestCase("feb", 2, 2014)]
        [TestCase("mar", 3, 2014)]
        [TestCase("apr", 4, 2014)]
        [TestCase("may", 5, 2014)]
        [TestCase("jun", 6, 2014)]
        [TestCase("jul", 7, 2014)]
        [TestCase("aug", 8, 2014)]
        [TestCase("sep", 9, 2013)]
        [TestCase("oct", 10, 2013)]
        [TestCase("nov", 11, 2013)]
        [TestCase("dec", 12, 2013)]
        public void exact_times_with_months(string month, int number, int year)
        {
            var now = new DateTime(2013, 08, 31, 11, 0, 0);
            var result = ReminderParser.Parse("do stuff on " + month + " 26 at 12:30pm", now);
            result.Description.ShouldBe("do stuff");
            result.ReminderDate.ShouldBe(new DateTime(year, number, 26, 12, 30, 0));

            var result2 = ReminderParser.Parse("on " + month + " 26 at 12:30pm do stuff", now);
            result2.Description.ShouldBe("do stuff");
            result2.ReminderDate.ShouldBe(new DateTime(year, number, 26, 12, 30, 0));

            var result3 = ReminderParser.Parse("do stuff at 12:30pm on " + month + " 26", now);
            result3.Description.ShouldBe("do stuff");
            result3.ReminderDate.ShouldBe(new DateTime(year, number, 26, 12, 30, 0));
        }

        [TestCase("1st", 1)]
        [TestCase("2nd", 2)]
        [TestCase("3rd", 3)]
        [TestCase("19th", 19)]
        public void handles_day_suffixes(string text, int day)
        {
            var now = new DateTime(2013, 08, 31, 11, 0, 0);
            var result = ReminderParser.Parse("do stuff on Oct " + text + " at 12:30pm", now);
            result.ReminderDate.ShouldBe(new DateTime(2013, 10, day, 12, 30, 0));
            result.Description.ShouldBe("do stuff");
        }

        [TestCase("first", 1)]
        [TestCase("second", 2)]
        [TestCase("third", 3)]
        [TestCase("fourth", 4)]
        [TestCase("fifth", 5)]
        [TestCase("sixth", 6)]
        [TestCase("seventh", 7)]
        [TestCase("eighth", 8)]
        [TestCase("ninth", 9)]
        [TestCase("tenth", 10)]
        [TestCase("eleventh", 11)]
        [TestCase("twelfth", 12)]
        [TestCase("thirteenth", 13)]
        [TestCase("fourteenth", 14)]
        [TestCase("fifteenth", 15)]
        [TestCase("sixteenth", 16)]
        [TestCase("seventeenth", 17)]
        [TestCase("eighteenth", 18)]
        [TestCase("nineteenth", 19)]
        [TestCase("twentieth", 20)]
        [TestCase("twentyfirst", 21)]
        [TestCase("twenty-second", 22)]
        [TestCase("twentythird", 23)]
        [TestCase("twentyfourth", 24)]
        [TestCase("twenty-fifth", 25)]
        [TestCase("twentysixth", 26)]
        [TestCase("twentyseventh", 27)]
        [TestCase("twentyeighth", 28)]
        [TestCase("twenty-ninth", 29)]
        [TestCase("thirtieth", 30)]
        [TestCase("thirtyfirst", 31)]
        public void day_number_names_converted_to_numbers(string day, int dayNumber)
        {
            var now = new DateTime(2013, 08, 1, 14, 9, 0);
            var result = ReminderParser.Parse("do important stuff on oct " + day + " at 9am ", now);
            result.Description.ShouldBe("do important stuff");
            result.ReminderDate.ShouldBe(new DateTime(2013, 10, dayNumber, 9, 0, 0));
        }
        
        [Test]
        public void month_not_parsed_properly()
        {
            var now = new DateTime(2013, 09, 1, 14, 9, 0);
            var result = ReminderParser.Parse("do important stuff on sept 2 at 9am ", now);
            result.Description.ShouldBe("do important stuff");
            result.ReminderDate.ShouldBe(new DateTime(2013, 09, 2, 9, 0, 0));
        }

        [Test]
        public void handles_month_in_message()
        {
            var now = new DateTime(2013, 09, 6, 14, 9, 0);
            var result = ReminderParser.Parse("do stuff for september on wednesday at 2:15pm", now);
            result.Description.ShouldBe("do stuff for september");
            result.ReminderDate.Day.ShouldBe(11);
        }
    }
}