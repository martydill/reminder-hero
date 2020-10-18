using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReminderHero.Models
{

    public class GoogleAccount 
    {
        public GoogleAccount()
        {
            Calendars = new Collection<GoogleCalendar>();
        }

        [Key]
        public int Id { get; set; }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public DateTime ExpiryDate { get; set; }

        public string TokenType { get; set; }

        public virtual ICollection<GoogleCalendar> Calendars { get; set; }
    }
}