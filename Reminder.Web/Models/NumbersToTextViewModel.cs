using System.Collections.Generic;
using ReminderHero.Models;

namespace ReminderHero.Web.Controllers
{
    public class NumbersToTextViewModel
    {
        public IEnumerable<PhoneNumber> Numbers { get; set; }
    }
}