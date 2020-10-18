using System.Collections.Generic;
using ReminderHero.Models;

namespace ReminderHero.Web.Controllers
{
    public class EndpointsViewModel 
    {
        public string ThingName { get; set; }
        public EndpointType EndpointType { get; set; }
        public IEnumerable<Endpoint> Endpoints { get; set; }

        public bool CanAddMore { get; set; }
        public bool CanUseAtAll { get; set; }
    }
}