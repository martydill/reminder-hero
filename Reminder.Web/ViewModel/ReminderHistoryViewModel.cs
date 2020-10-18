using System;
using System.Collections.Generic;
using NodaTime;
using ReminderHero.Models;

namespace ReminderHero.Web.Controllers
{
    public class ReminderHistoryDisplay
    {
        public string Description { get; set; }

        public DateTime ReminderDate { get; set; }

        public Recurrence Recurrence
        {
            get { return (Recurrence) RecurrenceInt; }
        }

        public string Address { get; set; }

        public int RecurrenceInt { get; set; }

        public string TimeZone { get; set; }

        public DateTime LocalDate
        {
            get
            {
                var tz = DateTimeZoneProviders.Tzdb[TimeZone];
                var utc = DateTime.SpecifyKind(ReminderDate, DateTimeKind.Utc);
                var instant = Instant.FromDateTimeUtc(utc);
                var inZone = instant.InZone(tz);
                return inZone.ToDateTimeUnspecified();
            }
        }
    }

    public class ReminderHistoryViewModel
    {
        public IEnumerable<ReminderHistoryDisplay> Reminders { get; set; }
    }
}