using System.IO;
using System.Web.Mvc;
using ReminderHero.Models;
using ReminderHero.Web.Mailers;
using ReminderHero.Web.Models;

namespace ReminderHero.Web.Controllers
{
    public class StripeController : Controller
    {
        private readonly IUserMailer _emailSender;

        public StripeController(IUserMailer emailSender)
        {
            _emailSender = emailSender;
        }

        public ActionResult Process()
        {
            var json = new StreamReader(Request.InputStream).ReadToEnd();
            using (var ctx = new RemindersContext())
            {
                var processor = new StripeProcessor(ctx, _emailSender);
                processor.Process(json);
            }

            return new HttpStatusCodeResult(200);
        }
    }
}
