using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReminderHero.Models
{

    public class GoogleCalendar
    {
        [Key]
        public int Id { get; set; }

        public int GoogleAccountId { get; set; }

        public string GoogleId { get; set; }

        public string Summary { get; set; }

        public bool SendEventsToMe { get; set; }

        public virtual GoogleAccount GoogleAccount { get; set; }
    }
}