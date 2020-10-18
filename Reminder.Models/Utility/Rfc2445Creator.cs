using System;
using System.Text;

namespace ReminderHero.Models.Utility
{
    public static class Rfc2445Creator
    {
        public static string From(Reminder r)
        {
            var sb = new StringBuilder();
            sb.Append("RRULE:FREQ=");
            switch(r.Recurrence)
            {
                case Recurrence.Daily:
                    sb.Append("DAILY");
                    break;

                case Recurrence.Weekly:
                    sb.Append("WEEKLY");
                    break;

                case Recurrence.Monthly:
                    sb.Append("MONTHLY");
                    break;

                case Recurrence.Yearly:
                    sb.Append("YEARLY");
                    break;

                case Recurrence.EveryXDays:
                    sb.Append("DAILY;INTERVAL=" + r.RecurrencePeriod + ";");
                    break;

                case Recurrence.EveryXWeeks:
                    sb.Append("WEEKLY;INTERVAL=" + r.RecurrencePeriod + ";");
                    break;

                case Recurrence.EveryXMonths:
                    sb.Append("MONTHLY;INTERVAL=" + r.RecurrencePeriod + ";");
                    break;

                case Recurrence.EveryXYears:
                    sb.Append("YEARLY;INTERVAL=" + r.RecurrencePeriod + ";");
                    break;

                case Recurrence.Weekday:
                    sb.Append("WEEKLY;BYDAY=MO,TU,WE,TH,FR;");
                    break;

                case Recurrence.Weekend:
                    sb.Append("WEEKLY;BYDAY=SA;");
                    break;

                default:
                    throw new ArgumentOutOfRangeException("r.recurrence");
            }

            return sb.ToString();
        }
    }
}
