using System.Collections.Generic;
using System.Linq;
using ReminderHero.Models;

namespace ReminderHero.Web.Controllers
{
    public class GoogleCalendarViewModel
    {
        public GoogleCalendarViewModel()
        {
        }

        public GoogleCalendarViewModel(GoogleAccount acct, IEnumerable<GoogleCalendar> calendars)
        {
            Calendars = calendars;
            Connected = acct != null;
        }

        public bool Connected { get; set; }

        public IEnumerable<GoogleCalendar> Calendars { get; private set; }
    }
}