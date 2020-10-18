using System;
using NUnit.Framework;
using ReminderHero.Models;
using ReminderHero.Parser;
using Shouldly;

namespace ReminderHero.Data.Tests
{
    public class ReminderParserTest
    {
        [Test]
        public void full_test()
        {
            var result = ReminderParser.Parse("Remind me to take out the trash in 1 day", DateTime.Now);
            result.Description.ShouldBe("to take out the trash");
            (result.ReminderDate - result.CreatedDate).TotalDays.ShouldBe(1);
        }

        [Test]
        public void full_test_2()
        {
            var result = ReminderParser.Parse("Remind me about my appointment in three hours", DateTime.Now);
            result.Description.ShouldBe("about your appointment");
            (result.ReminderDate - result.CreatedDate).TotalHours.ShouldBe(3);
        }

        [Test]
        public void removes_remind_me_after_reminder_time()
        {
            var result = ReminderParser.Parse("in three hours remind me about my appointment", DateTime.Now);
            result.Description.ShouldBe("about your appointment");
            (result.ReminderDate - result.CreatedDate).TotalHours.ShouldBe(3);
        }

        [Test]
        public void blah()
        {
            var result = ReminderParser.Parse("Look at the house in an hour ", DateTime.Now);
            result.Description.ShouldBe("Look at the house");
            result.ReminderDate.Subtract(result.CreatedDate).Hours.ShouldBe(1);
        }

        [Test]
        public void handles_whitespace_and_punctuationfull_test()
        {
            var result = ReminderParser.Parse(" Remind me to take out the trash in 1 day.,   ", DateTime.Now);
            result.Description.ShouldBe("to take out the trash");
            (result.ReminderDate - result.CreatedDate).TotalDays.ShouldBe(1);
        }

        [Test]
        public void replaces_my_with_your()
        {
            var result = ReminderParser.Parse("Remind me to give my friend my keys in 1 hour", DateTime.Now);
            result.Description.ShouldBe("to give your friend your keys");
        }

        [Test]
        public void replaces_i_with_you()
        {
            var result = ReminderParser.Parse("i have to go to work in an hour", DateTime.Now);
            result.Description.ShouldBe("you have to go to work");
        }

        [Test]
        public void removes_and()
        {
            var result = ReminderParser.Parse("and to send tim the invoice an hour", DateTime.Now);
            result.Description.ShouldBe("to send tim the invoice");
        }

        [Test]
        public void works_without_remind_me()
        {
            var result = ReminderParser.Parse("Return my library book in a day", DateTime.Now);
            result.Description.ShouldBe("Return your library book");
        }

        [TestCase("Remind me to do stuff in 1 minute", "to do stuff")]
        [TestCase("remind me about important stuff in 1 minute", "about important stuff")]
        public void handles_reminder_variations(string text, string rest)
        {
            var result = ReminderParser.Parse(text, DateTime.Now);
            result.Description.ShouldBe(rest);
        }
        
        [TestCase("to do stuff in 1 hour     <http://hu.linkedin.com/in/xxxyyyzzzttt> ")]
        [TestCase("to do stuff in 1 hour.    - bob smith)]
        [TestCase("to do stuff in 1 hour    -------------------     Stephen  555-555-5555    ")]
        [TestCase("to do stuff in 1 hour\nthanks, marty")]
        [TestCase("to do stuff in 1 hour. cheers, bob")]
        public void handles_signatures(string msg)
        {
            var result = ReminderParser.Parse(msg, DateTime.Now);
            result.Description.ShouldBe("to do stuff");
            result.ReminderDate.ShouldBe(result.CreatedDate.AddHours(1));
        }

        [Test]
        public void handles_time_before_message()
        {
            var result = ReminderParser.Parse("in 1 hour to do stuff", DateTime.Now);
            result.Description.ShouldBe("to do stuff");
            result.ReminderDate.ShouldBe(result.CreatedDate.AddHours(1));
        }

        [Test]
        public void handles_day_of_week_words()
        {
            var now = new DateTime(2013, 09, 6, 14, 9, 0);
            var result = ReminderParser.Parse("do stuff for tuesday on wednesday at 2:15pm", now);
            result.Description.ShouldBe("do stuff for tuesday");
            result.ReminderDate.Day.ShouldBe(11);
        }

        [TestCase("in 5m", 0, 0, 5, 0)]
        [TestCase("in 5 m", 0, 0, 5, 0)]
        [TestCase("in 5 M", 0, 0, 5, 0)]
        [TestCase("in 15min", 0, 0, 15, 0)]
        [TestCase("in 7 min", 0, 0, 7, 0)]
        [TestCase("in 15 minutes", 0, 0, 15, 0)]
        [TestCase("in an hour", 0, 1, 0, 0)]
        [TestCase("in 2 hours", 0, 2, 0, 0)]
        [TestCase("in 2 hr", 0, 2, 0, 0)]
        [TestCase("in 2h", 0, 2, 0, 0)]
        [TestCase("in 2.5h", 0, 0, 150, 0)] 
        [TestCase("in 2 h", 0, 2, 0, 0)]
        [TestCase("in a day", 1, 0, 0, 0)]
        [TestCase("in two days", 2, 0, 0, 0)]
        [TestCase("in 3d", 3, 0, 0, 0)]
        [TestCase("in 3 d", 3, 0, 0, 0)]
        [TestCase("in two weeks", 0, 0, 0, 2)]
        [TestCase("in one wk", 0, 0, 0, 1)]
        [TestCase("in one w", 0, 0, 0, 1)]
        public void handles_in_x_y(string text, int days, int hours, int minutes, int weeks = 0)
        {
            var ts = ReminderParser.ParseTimespan(text.Split(' '), null, "");
            var expected = new TimeSpan(days + weeks * 7, hours, minutes, 0);
            ts.TimeSpan.ShouldBe(expected);
        }

        [TestCase("LSDJFLSK DFLSK")]
        [TestCase("Remind me to do stuff in eleventy billion years")]
        [TestCase("Reimnd me to be somewhere at 9 past 9 past 9")]
        public void throws_exceptions_when_data_is_bad(string text)
        {
            Should.Throw<Exception>(() => ReminderParser.Parse(text, DateTime.Now));
        }

        [Test]
        public void reminder_containing_the_word_in_doesnt_crash()
        {
            var now = new DateTime(2013, 09, 1, 14, 9, 0);
            var result = ReminderParser.Parse("Take car in for eleven fifteen appointment on sept 14 at 10am", now);
            result.ReminderDate.Day.ShouldBe(14);
        }

        [TestCase("tomorrow")]
        [TestCase("tonight")]
        [TestCase("this")]
        [TestCase("today")]
        [TestCase("each")]
        [TestCase("every")]
        public void reminder_containing_extra_words_doesnt_crash(string word)
        {
            var now = new DateTime(2013, 09, 1, 14, 9, 0);
            var result = ReminderParser.Parse("tape the " + word + " people on oct 9 at 9am", now);
            result.ReminderDate.Month.ShouldBe(10);
            result.ReminderDate.Day.ShouldBe(9);
            result.ReminderDate.Hour.ShouldBe(9);
        }

        [TestCase("Remind me to scan the pages and call the parent tomorrow at 6 p.m.")]
        [TestCase("Please remind me to call the parent and scan the pages tomorrow at 6 pm.")]
        [TestCase("Remind me to call the parent and scan the pages tomorrow at 6 pm.")]
        public void angelas_reminders(string word)
        {
            var now = new DateTime(2013, 10, 8, 18, 56, 0);
            var result = ReminderParser.Parse(word, now);
            result.ReminderDate.Month.ShouldBe(10);
            result.ReminderDate.Day.ShouldBe(9);
            result.ReminderDate.Hour.ShouldBe(18);
        }

        [Test]
        public void period_at_end_of_word()
        {
            var result = ReminderParser.Parse("Remind me to retire tonight.", DateTime.Now);
            result.ReminderDate.Hour.ShouldBe(ReminderParser.NightHour);
        }


        [Test]
        public void day_after_time()
        {
            var result = ReminderParser.Parse("Call home at 8pm on Saturday  ", DateTime.Now);
            result.ReminderDate.Hour.ShouldBe(20);
        }

        [Test]
        public void day_after_time_with_at_symbol()
        {
            var result = ReminderParser.Parse("Call home @ 9 am on Sunday", DateTime.Now);
            result.ReminderDate.Hour.ShouldBe(9);
        }

        [Test]
        public void day_after_time_with_specific_day()
        {
            var now = new DateTime(2013, 11, 1, 12, 56, 0);
            var result = ReminderParser.Parse("Dentist appointment at 11:30am on Nov 13", now);
            result.ReminderDate.Hour.ShouldBe(11);
            result.ReminderDate.Day.ShouldBe(13);
        }

        [Test]
        public void reminder_containing_in_a()
        {
            var result = ReminderParser.Parse("Put the thing in a box in 2h", DateTime.Now);
            result.Description.ShouldBe("Put the thing in a box");
            result.ReminderDate.ShouldBe(result.CreatedDate.AddHours(2));
        }

        [Test]
        public void day_after_time_tomorrow()
        {  
            var now = new DateTime(2013, 11, 1, 12, 56, 0);
            var result = ReminderParser.Parse("Call home @ 9 am tomorrow", now);
            result.ReminderDate.Hour.ShouldBe(9);
            result.ReminderDate.Day.ShouldBe(2);
        }

        [TestCase(" Create conference questionnaire at 14:00    ")]
        [TestCase("   Create conference questionnaire at 14:00 today       ")]
        [TestCase("    Create conference questionnaire @ 2PM today    ")]
        public void reminders_ending_in_today(string s)
        {
            var result = ReminderParser.Parse(s, DateTime.Now);
            result.ReminderDate.Hour.ShouldBe(14);
        }

        [Test]
        public void another_day_after_time_quotes_with()
        {
            var result = ReminderParser.Parse("REminder me eery day at 8 am \"I am good\"", DateTime.Now);
            result.ReminderDate.Hour.ShouldBe(8);
            result.Recurrence.ShouldBe(Recurrence.Daily);
            result.Description.ShouldBe( "\"I am good\"");
        }

        [TestCase("Pick up Tommy @ 6:30pm")]
        [TestCase("Pick up Tommy @6:30pm")]
        public void at_sign_instead_of_at(string desc)
        {
            var result = ReminderParser.Parse(desc, DateTime.Now);
            result.ReminderDate.Hour.ShouldBe(18);
            result.ReminderDate.Minute.ShouldBe(30);
            result.Description.ShouldBe("Pick up Tommy");
        }

        [TestCase("Call John at 15:00pm ")]
        [TestCase("Call John at 15:00 pm ")]
        [TestCase("Call John at 15 pm ")]
        public void twentyfour_hour_time_with_ampm(string desc)
        {
            var result = ReminderParser.Parse(desc, DateTime.Now);
            result.ReminderDate.Hour.ShouldBe(15);
            result.Description.ShouldBe("Call John");
        }

        [Test]
        [Ignore]
        public void reminder_without_at()
        {
            var result =
                ReminderParser.Parse(
                    "  2013-10-22 03.00PM  bc def     ccc ddd eee fff  freelance webdesign | stuff| stuff| pr    ",
                    DateTime.Now);

        }
    }
}
