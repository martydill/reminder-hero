using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using NodaTime;
using ReminderHero.Models;

namespace ReminderHero.Web.Controllers
{
    public class ReminderSettingsViewModel
    {
        [Display(Name = "First Reminder")]
        [Range(1, 999, ErrorMessage = "Must be a number between 1 and 999")]
        public int Reminder1Number { get; set; }

        public ReminderPeriod Reminder1Period { get; set; }

        [Display(Name = "Second Reminder")]
        [Range(1, 999, ErrorMessage = "Must be a number between 1 and 999")]
        public int Reminder2Number { get; set; }

        public ReminderPeriod Reminder2Period { get; set; }

        [Display(Name = "Third Reminder")]
        [Range(1, 999, ErrorMessage = "Must be a number between 1 and 999")]
        public int Reminder3Number { get; set; }

        public ReminderPeriod Reminder3Period { get; set; }

        [Display(Name = "Fourth Reminder")]
        [Range(1, 999, ErrorMessage = "Must be a number between 1 and 999")]
        public int Reminder4Number { get; set; }

        public ReminderPeriod Reminder4Period { get; set; }

        public bool Reminder1Enabled { get; set; }

        public bool Reminder2Enabled { get; set; }

        public bool Reminder3Enabled { get; set; }

        public bool Reminder4Enabled { get; set; }
        public IEnumerable<SelectListItem> Periods { get; set; }

        public SelectListItem Period1 { get; set; }

        public SelectListItem Period2 { get; set; }

        public SelectListItem Period3 { get; set; }

        public SelectListItem Period4 { get; set; }
        public bool IsSave { get; set; }


        public ReminderSettingsViewModel()
        {
            BuildPeriods();
        }

        public ReminderSettingsViewModel(User user)
        {
            BuildPeriods();

            Reminder1Enabled = user.Reminder1Enabled;
            Reminder1Number = NumberFor(user.Reminder1Number, user.Reminder1Period);
            Reminder1Period = user.Reminder1Period;
            Period1 = Periods.FirstOrDefault(p => p.Value == user.Reminder1Period.ToString());

            Reminder2Enabled = user.Reminder2Enabled;
            Reminder2Number = NumberFor(user.Reminder2Number, user.Reminder2Period);
            Reminder2Period = user.Reminder2Period;
            Period2 = Periods.FirstOrDefault(p => p.Value == user.Reminder2Period.ToString());

            Reminder3Enabled = user.Reminder3Enabled;
            Reminder3Number = NumberFor(user.Reminder3Number, user.Reminder3Period);
            Reminder3Period = user.Reminder3Period;
            Period3 = Periods.FirstOrDefault(p => p.Value == user.Reminder3Period.ToString());

            Reminder4Enabled = user.Reminder4Enabled;
            Reminder4Number = NumberFor(user.Reminder4Number, user.Reminder4Period);
            Reminder4Period = user.Reminder4Period;
            Period4 = Periods.FirstOrDefault(p => p.Value == user.Reminder4Period.ToString());
        }

        private int NumberFor(int number, ReminderPeriod period)
        {
            if (period == ReminderPeriod.Minutes)
                return number;
            else if (period == ReminderPeriod.Hours)
                return number / 60;
            else if (period == ReminderPeriod.Days)
                return number / (3600);
            else if (period == ReminderPeriod.Weeks)
                return number / (3600 * 7);
            else
                throw new ArgumentOutOfRangeException("period");
        }

        private void BuildPeriods()
        {
            Periods = new List<SelectListItem>()
            {
                new SelectListItem() {Text = "minutes", Value = ReminderPeriod.Minutes.ToString()},
                new SelectListItem() {Text = "hours", Value = ReminderPeriod.Hours.ToString()},
                new SelectListItem() {Text = "days", Value = ReminderPeriod.Days.ToString()},
                new SelectListItem() {Text = "weeks", Value = ReminderPeriod.Weeks.ToString()},
            };
        }

        public void CopySettingsToUser(User user)
        {
            user.Reminder1Enabled = Reminder1Enabled;
            user.Reminder1Number = NumberFrom(Reminder1Number, Reminder1Period);
            user.Reminder1Period = PeriodFrom(Reminder1Period);

            user.Reminder2Enabled = Reminder2Enabled;
            user.Reminder2Number =  NumberFrom(Reminder2Number, Reminder2Period);;
            user.Reminder2Period = PeriodFrom(Reminder2Period);

            user.Reminder3Enabled = Reminder3Enabled;
            user.Reminder3Number =  NumberFrom(Reminder3Number, Reminder3Period);;
            user.Reminder3Period = PeriodFrom(Reminder3Period);

            user.Reminder4Enabled = Reminder4Enabled;
            user.Reminder4Number =  NumberFrom(Reminder4Number, Reminder4Period);
            user.Reminder4Period = PeriodFrom(Reminder4Period);
        }

        private int NumberFrom(int number, ReminderPeriod period)
        {
              if (period == ReminderPeriod.Minutes)
                return number;
            else if (period == ReminderPeriod.Hours)
                return number *  60;
            else if (period == ReminderPeriod.Days)
                return number *  (3600);
            else if (period == ReminderPeriod.Weeks)
                return number * (3600 * 7);
            else
                throw new ArgumentOutOfRangeException("period");
        }

        private ReminderPeriod PeriodFrom(ReminderPeriod selected)
        {
            if (selected == ReminderPeriod.Minutes)
                return ReminderPeriod.Minutes;

            if (selected == ReminderPeriod.Hours)
                return ReminderPeriod.Hours;

            if (selected == ReminderPeriod.Days)
                return ReminderPeriod.Days;

            if (selected == ReminderPeriod.Weeks)
                return ReminderPeriod.Weeks;

            return ReminderPeriod.Minutes;
        }
    }
}