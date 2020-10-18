using System.Collections.Generic;
using ReminderHero.Models;

namespace ReminderHero.Web.Controllers
{
    public class RemindersViewModel
    {
        public IEnumerable<Reminder> Reminders { get; set; }
    }
}