using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Objects;
using System.Linq;
using NodaTime;

namespace ReminderHero.Models
{
    public enum Recurrence
    {
        Once = 0,
        Daily = 1,
        Weekly = 2,
        Monthly = 3,
        Yearly = 4,
        Weekend = 5,
        Weekday = 6,
        EveryXDays = 7,
        EveryXWeeks = 8,
        EveryXMonths = 9,
        EveryXYears = 10,
    }

    [Table("Reminder")]
    public class Reminder
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string To { get; set; }

        public string Email { get; set; }
        public string SourceMessage { get; set; }
        public string SourceTitle { get; set; }
        public string Description { get; set; }
        public DateTime ReminderDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool Handled { get; set; }

        public bool Reminder1Fired { get; set; }

        public bool Reminder2Fired { get; set; }

        public bool Reminder3Fired { get; set; }

        public bool Reminder4Fired { get; set; }

        public int RecurrenceType { get; set; }
        public int RecurrencePeriod { get; set; }

        [ForeignKey("User")]
        public Guid? UserId { get; set; }

        public virtual User User { get; set; }

        [NotMapped]
        public bool IsTimeRelative { get; set; }

        [NotMapped]
        public Recurrence Recurrence
        {
            get { return (Recurrence)RecurrenceType; }

            set { RecurrenceType = (int)value; }
        }

        public string GoogleIds { get; set; }

        [NotMapped]
        public string RecurrenceAsFriendlyString
        {
            get
            {
                switch (Recurrence)
                {
                    case Recurrence.EveryXDays:
                        return String.Format("Every {0} Days", RecurrencePeriod);
                    case Recurrence.EveryXWeeks:
                        return String.Format("Every {0} Weeks", RecurrencePeriod);
                    case Recurrence.EveryXMonths:
                        return String.Format("Every {0} Months", RecurrencePeriod);
                    default:
                        return Recurrence.ToString();
                }
            }
        }

        public void HandleDelivery(HandleReminderType type)
        {
            var start = ReminderDate;
            switch (type)
            {
                case HandleReminderType.Real:

                    switch (Recurrence)
                    {
                        case Recurrence.Once:
                            Handled = true;
                            break;

                        case Recurrence.Daily:
                            ReminderDate = ReminderDate.AddDays(1);
                            break;

                        case Recurrence.Weekly:
                            ReminderDate = ReminderDate.AddDays(7);
                            break;

                        case Recurrence.Weekend:
                            ReminderDate = ReminderDate.AddDays(7);
                            break;

                        case Recurrence.Yearly:
                            ReminderDate = ReminderDate.AddYears(1);
                            break;

                        case Recurrence.Weekday:
                            if (ReminderDate.DayOfWeek != DayOfWeek.Friday)
                                ReminderDate = ReminderDate.AddDays(1);
                            else
                                ReminderDate = ReminderDate.AddDays(3);
                            break;

                        case Recurrence.EveryXDays:
                            ReminderDate = ReminderDate.AddDays(RecurrencePeriod);
                            break;

                        case Recurrence.EveryXWeeks:
                            ReminderDate = ReminderDate.AddDays(RecurrencePeriod * 7);
                            break;

                        case Recurrence.EveryXMonths:
                            ReminderDate = ReminderDate.AddMonths(RecurrencePeriod);
                            break;

                        case Recurrence.EveryXYears:
                            ReminderDate = ReminderDate.AddYears(RecurrencePeriod);
                            break;

                        default:
                            throw new Exception(String.Format("Unknown recurrence type {0}", Recurrence));
                    }

                    var end = ReminderDate;
                    if (User != null)
                    {
                        var tz = DateTimeZoneProviders.Tzdb[User.TimeZone];
                        var startZoneInterval = tz.GetZoneInterval(Instant.FromDateTimeUtc(new DateTime(start.Ticks, DateTimeKind.Utc)));
                        var endZoneInterval = tz.GetZoneInterval(Instant.FromDateTimeUtc(new DateTime(end.Ticks, DateTimeKind.Utc)));
                        if (startZoneInterval.WallOffset != endZoneInterval.WallOffset)
                        {
                            ReminderDate = ReminderDate.Add((startZoneInterval.WallOffset - endZoneInterval.WallOffset).ToTimeSpan());
                        }
                    }

                    if (Recurrence != Models.Recurrence.Once && User != null)
                    {
                        Reminder1Fired = start > ReminderDate.AddMinutes(-(float) User.Reminder1Number) &&
                                         User.Reminder1Enabled;
                        Reminder2Fired = start > ReminderDate.AddMinutes(-(float) User.Reminder2Number) &&
                                         User.Reminder2Enabled;
                        Reminder3Fired = start > ReminderDate.AddMinutes(-(float) User.Reminder3Number) &&
                                         User.Reminder3Enabled;
                        Reminder4Fired = start > ReminderDate.AddMinutes(-(float) User.Reminder4Number) &&
                                         User.Reminder4Enabled;
                    }
                    break;

                case HandleReminderType.One:
                    Reminder1Fired = true;

                    break;

                case HandleReminderType.Two:
                    Reminder2Fired = true;
                    break;

                case HandleReminderType.Three:
                    Reminder3Fired = true;
                    break;

                case HandleReminderType.Four:
                    Reminder4Fired = true;
                    break;

                default:
                    throw new ArgumentOutOfRangeException("type");
            }

        }

        [NotMapped]
        public DateTime LocalCreatedDate
        {
            get
            {
                var tz = DateTimeZoneProviders.Tzdb[TimeZone];
                var utc = DateTime.SpecifyKind(CreatedDate, DateTimeKind.Utc);
                var instant = Instant.FromDateTimeUtc(utc);
                var inZone = instant.InZone(tz);
                return inZone.ToDateTimeUnspecified();
            }
        }

        [NotMapped]
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

        [NotMapped]
        public string TimeZone { get; set; }

        public int Type { get; set; }

        [NotMapped]
        public EndpointType ReminderType
        {
            get { return (EndpointType)Type; }
            set { Type = (int)value; }
        }

        [NotMapped]
        public bool HasTime { get; set; }

        [NotMapped]
        public bool HasDate { get; set; }


        public static IEnumerable<Tuple<HandleReminderType, Reminder>> FindRemindersToHandle(IDbSet<Reminder> reminderSet, DateTime time)
        {
            var remindersToHandle = reminderSet.Where(r => !r.Handled &&
                        r.ReminderDate < time
                        ).Include("User").ToList();


            var r1 = reminderSet.Where(r => !r.Handled && r.User != null &&
                                            r.User.Reminder1Enabled && !r.Reminder1Fired &&
                                            EntityFunctions.AddMinutes(r.ReminderDate, -r.User.Reminder1Number) < time)
                .Include("User")
                .ToList();

            var r2 = reminderSet.Where(r => !r.Handled && r.User != null &&
                                            r.User.Reminder2Enabled && !r.Reminder2Fired &&
                                            EntityFunctions.AddMinutes(r.ReminderDate, -r.User.Reminder2Number) < time)
                .Include("User")
                .ToList();

            var r3 = reminderSet.Where(r => !r.Handled && r.User != null &&
                                            r.User.Reminder3Enabled && !r.Reminder3Fired &&
                                           EntityFunctions.AddMinutes(r.ReminderDate, -r.User.Reminder3Number) < time)
                .Include("User")
                .ToList();

            var r4 = reminderSet.Where(r => !r.Handled && r.User != null &&
                                            r.User.Reminder4Enabled && !r.Reminder4Fired &&
                                           EntityFunctions.AddMinutes(r.ReminderDate, -r.User.Reminder4Number) < time)
                .Include("User")
                .ToList();

            return remindersToHandle.Select(x => new Tuple<HandleReminderType, Reminder>(HandleReminderType.Real, x)).Concat(
                r1.Select(y => new Tuple<HandleReminderType, Reminder>(HandleReminderType.One, y)).Concat(
                r2.Select(y => new Tuple<HandleReminderType, Reminder>(HandleReminderType.Two, y)).Concat(
                r3.Select(y => new Tuple<HandleReminderType, Reminder>(HandleReminderType.Three, y)).Concat(
                r4.Select(y => new Tuple<HandleReminderType, Reminder>(HandleReminderType.Four, y))
                ))));
        }

        public string GetTimeFor(HandleReminderType handleReminderType)
        {
            string period = "";
            int number = 0;

            switch(handleReminderType)
            {
                case HandleReminderType.One:
                    period = User.Reminder1Period.ToString();
                    number = NumberFor(User.Reminder1Number, User.Reminder1Period);
                    break;
                case HandleReminderType.Two:
                    period = User.Reminder2Period.ToString();
                    number = NumberFor(User.Reminder2Number, User.Reminder2Period);
                    break;
                case HandleReminderType.Three:
                    period = User.Reminder3Period.ToString();
                    number = NumberFor(User.Reminder3Number, User.Reminder3Period);
                    break;
                case HandleReminderType.Four:
                    period = User.Reminder4Period.ToString();
                    number = NumberFor(User.Reminder4Number, User.Reminder4Period);
                    break;
                default:
                    return String.Empty;
            }

            period = period.ToLower();
            return number + " " + (number == 1 ? period.TrimEnd('s') : period);
        }

        private int NumberFor(int reminder1Number, ReminderPeriod reminder1Period)
        {
            switch(reminder1Period)
            {
                case ReminderPeriod.Minutes:
                    return reminder1Number;
                case ReminderPeriod.Hours:
                    return reminder1Number / 60;
                case ReminderPeriod.Days:
                    return reminder1Number / (60 * 24);
                case ReminderPeriod.Weeks:
                    return reminder1Number / (60 * 24 * 7);
                default:
                    throw new ArgumentOutOfRangeException("reminder1Period");
            }
        }
    }

    public enum HandleReminderType
    {
        Real,
        One,
        Two,
        Three,
        Four
    }
}