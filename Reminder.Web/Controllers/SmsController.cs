using System;
using System.Linq;
using System.Web.Mvc;
using ReminderHero.Models;
using ReminderHero.Web.Models;

namespace ReminderHero.Web.Controllers
{
    public class SmsController : Controller 
    {
        [System.Web.Http.HttpGet]
        public void IncomingMessage()
        {
          
        }
        
        [System.Web.Mvc.HttpPost]
        public ActionResult Receive()
        {
            try
            {
                var from = Request["From"];
                var message = Request["Body"];
                var to = Request["To"];

                using (var ctx = new RemindersContext())
                {
                    string data = new System.IO.StreamReader(Request.InputStream).ReadToEnd();

                    var request = new ReminderRequest();
                    request.CreatedDate = DateTime.UtcNow;
                    request.RawText = data;
                    request.RequestType = ReminderRequest.Sms;

                    ctx.ReminderRequests.Add(request);
                    ctx.SaveChanges();

                    var strippedFrom = Endpoint.GenerateStrippedAddressFrom(from, EndpointType.Phone).ToLower();
                    var fromWithoutCountryCode = RemoveCountryCodeFrom(from);
                    var endpoint = ctx.Endpoints.SingleOrDefault(e =>
                        (e.StrippedAddress.ToLower().Equals(strippedFrom)
                            || e.Address.ToLower().Equals(strippedFrom)
                            || e.StrippedAddress.ToLower().Equals(fromWithoutCountryCode))
                        && e.Type == (int)EndpointType.Phone);

                    ReminderRequestHandler.Build(from, message, String.Empty, String.Empty, ReminderHero.Models.EndpointType.Phone, to, endpoint);
                }
            }
            catch (Exception e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(e);
            }

            return Content("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<Response></Response>", "text/xml");
        }

        private string RemoveCountryCodeFrom(string from)
        {
            string[] countryCodes = new string[]{"+1", "+44", "+34" };

            foreach (string code in countryCodes)
            {
                if (from.StartsWith(code))
                    return from.Substring(code.Length, from.Length - code.Length);
            }

            return from;
        }
    }
}
