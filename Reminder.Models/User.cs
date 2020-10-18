using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReminderHero.Models
{
    public enum ReminderPeriod
    {
        Minutes,
        Hours,
        Days,
        Weeks
    }

    public class User
    {
        [Key]
        public virtual Guid UserId { get; set; }

        [Required]
        public virtual String Username { get; set; }

        public virtual String Email { get; set; }

        [Required, DataType(DataType.Password)]
        public virtual String Password { get; set; }

        public virtual String FirstName { get; set; }
        public virtual String LastName { get; set; }

        [DataType(DataType.MultilineText)]
        public virtual String Comment { get; set; }

        public virtual Boolean IsApproved { get; set; }
        public virtual int PasswordFailuresSinceLastSuccess { get; set; }
        public virtual DateTime? LastPasswordFailureDate { get; set; }
        public virtual DateTime? LastActivityDate { get; set; }
        public virtual DateTime? LastLockoutDate { get; set; }
        public virtual DateTime? LastLoginDate { get; set; }
        public virtual String ConfirmationToken { get; set; }
        public virtual DateTime? CreateDate { get; set; }
        public virtual Boolean IsLockedOut { get; set; }
        public virtual DateTime? LastPasswordChangedDate { get; set; }
        public virtual String PasswordVerificationToken { get; set; }
        public virtual DateTime? PasswordVerificationTokenExpirationDate { get; set; }

        public virtual ICollection<Role> Roles { get; set; }

        public string TimeZone { get; set; }

        public string StripeCustomerId { get; set; }

        public virtual int? PricePlanId { get; set; }

        public virtual PricePlan PricePlan { get; set; }

        public DateTime? PlanStartDateUtc { get; set; }

        public bool IsBetaUser { get; set; }

        public int Reminder1Number { get; set; }

        public int Reminder1PeriodInt { get; set; }

        public int Reminder2Number { get; set; }

        public int Reminder2PeriodInt { get; set; }

        public int Reminder3Number { get; set; }

        public int Reminder3PeriodInt { get; set; }

        public int Reminder4Number { get; set; }

        public int Reminder4PeriodInt { get; set; }

        [NotMapped]
        public int Reminder1DisplayNumber { get; set; }

        [NotMapped]
        public int Reminder2DisplayNumber { get; set; }

        [NotMapped]
        public int Reminder3DisplayNumber { get; set; }

        [NotMapped]
        public int Reminder4DisplayNumber { get; set; }

        public bool Reminder1Enabled { get; set; }
        
        public bool Reminder2Enabled { get; set; }

        public bool Reminder3Enabled { get; set; }

        public bool Reminder4Enabled { get; set; }

        public virtual GoogleAccount GoogleAccount { get; set; }

        public int? GoogleAccountId { get; set; }

        [NotMapped]
        public ReminderPeriod Reminder1Period
        {
            get { return (ReminderPeriod) Reminder1PeriodInt; }
            set { Reminder1PeriodInt = (int) value; }
        }

        [NotMapped]
        public ReminderPeriod Reminder2Period
        {
            get { return (ReminderPeriod)Reminder2PeriodInt; }
            set { Reminder2PeriodInt = (int)value; }
        }

        [NotMapped]
        public ReminderPeriod Reminder3Period
        {
            get { return (ReminderPeriod)Reminder3PeriodInt; }
            set { Reminder3PeriodInt = (int)value; }
        }

        [NotMapped]
        public ReminderPeriod Reminder4Period
        {
            get { return (ReminderPeriod)Reminder4PeriodInt; }
            set { Reminder4PeriodInt = (int)value; }
        }
    }
}