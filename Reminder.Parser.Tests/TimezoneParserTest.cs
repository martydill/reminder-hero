using NUnit.Framework;
using Shouldly;

namespace ReminderHero.Data.Tests
{
    public class TimezoneParserTest
    {
        [TestCase("UT", 0, 0)]
        [TestCase("GMT", 0, 0)]
        [TestCase("EST", -5, 0)]
        [TestCase("EDT", -4, 0)]
        [TestCase("CST", -6, 0)]
        [TestCase("CDT", -5, 0)]
        [TestCase("MST", -7, 0)]
        [TestCase("MDT", -6, 0)]
        [TestCase("PST", -8, 0)]
        [TestCase("PDT", -7, 0)]
        [TestCase("-0700", -7, 0)]
        [TestCase("+1130", 11, 30)]
        [TestCase("+0200", 2, 0)]
        public void supported_timezone_formats(string s, int hours, int minutes)
        {
            var offset = TimezoneParser.ParseString("asldf asldkfj asdf " + s);
            offset.Hours.ShouldBe(hours);
            offset.Minutes.ShouldBe(minutes);
        }

        [TestCase(0, 0, 0)]
        [TestCase(-7, -7, 0)]
        [TestCase(11.5, 11, 30)]
        public void decimal_formats(decimal utcOffset, int hours, int minutes)
        {
            var offset = TimezoneParser.ParseDecimal(utcOffset);
            offset.Hours.ShouldBe(hours);
            offset.Minutes.ShouldBe(minutes);
        }
    }
}
