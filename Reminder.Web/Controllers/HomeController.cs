using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using ReminderHero.Models;
using ReminderHero.Web.Mailers;
using ReminderHero.Web.Models;

namespace ReminderHero.Web.Controllers
{
    public class HomeController : Controller
    {
        protected override void HandleUnknownAction(string actionName)
        {
            try
            {
                View(actionName).ExecuteResult(this.ControllerContext);
            }
            catch (Exception ex)
            {
                throw new HttpException(404, "Not Found", ex);
            }
        }

        public ActionResult Index(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public ActionResult Contact(ContactViewModel vm)
        {
            if (!String.IsNullOrEmpty(vm.LeaveBlank))
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            new UserMailer().Contact(vm).SendAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public HttpStatusCodeResult Check()
        {
            var time = DateTime.UtcNow.AddSeconds(30);
            using (var ctx = new RemindersContext())
            {
                var needsHandling = Reminder.FindRemindersToHandle(ctx.Reminders, time);

                foreach (var item in needsHandling)
                {
                    var type = item.Item1;
                    var r = item.Item2;
                    try
                    {
                        HandleReminder(r, type);

                        var rd = new ReminderDelivery();
                        rd.UserId = r.UserId;
                        rd.Date = DateTime.UtcNow;
                        rd.Reminder = r;
                        rd.Success = true;
                        rd.DeliveredTo = r.Email;
                        rd.IsPreReminder = type != HandleReminderType.Real;
                        ctx.ReminderDeliveries.Add(rd);

                        ctx.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        Elmah.ErrorSignal.FromCurrentContext().Raise(e);
                    }
                }
            }

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        private void HandleReminder(Reminder reminder, HandleReminderType type)
        {
            if (reminder.ReminderType == EndpointType.Email)
            {
                var m = new UserMailer();
                m.Reminder(reminder, type).SendAsync();
            }
            else if (reminder.ReminderType == EndpointType.Phone)
            {
                var x = new Twilio.TwilioRestClient("example",
                                                    "example");
                
                x.SendSmsMessage(reminder.To, reminder.Email, ("Reminder: " + reminder.Description + " " + reminder.GetTimeFor(type)).Trim());
            }
            else
            {
                throw new Exception(reminder.ReminderType.ToString());
            }

            reminder.HandleDelivery(type);
        }

        [HttpPost]
        public ActionResult ManualReminder(string reminder, string email, string subject)
        {
            using (var ctx = new RemindersContext())
            {
                var endpoint = ctx.Endpoints.Single(e => e.Address == email);
                ReminderRequestHandler.Build(email, subject, reminder, "", EndpointType.Email, String.Empty, endpoint);
                return View();
            }
        }

        [ValidateInput(false)]
        [HttpPost]
        public HttpStatusCodeResult ParseEmail()
        {
            try
            {
                byte[] data = new byte[Request.ContentLength];
                var x = Request.InputStream.Read(data, 0, data.Length);
                var y = Encoding.ASCII.GetString(data);
                var regex = new Regex("([^&=]+)=?([^&]*)", RegexOptions.Compiled);
                var matches = regex.Matches(y);
                if (matches.Count == 1 && matches[0].Groups[1].Value == "message")
                {
                    var msg = matches[0].Groups[2].Value;
                    var decoded = Server.UrlDecode(msg);
                    Response.Write(decoded);
                    return BuildString(decoded, data);
                }
            }
            catch (Exception e)
            {
                Response.Write(e.ToString());
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        private HttpStatusCodeResult BuildString(string message, byte[] data)
        {
            var request = new ReminderRequest();
            request.CreatedDate = DateTime.UtcNow;
            request.RawBytes = data;
            request.RawText = message;
            request.RequestType = ReminderRequest.Email;

            using (var ctx = new RemindersContext())
            {
                ctx.ReminderRequests.Add(request);
                ctx.SaveChanges();

                if (!String.IsNullOrEmpty(message))
                {
                    var x = new OpenPop.Mime.Message(Encoding.ASCII.GetBytes(message));

                    string textMessage = "";
                    string htmlMessage = "";
                    if (!x.MessagePart.IsMultiPart)
                    {
                        textMessage = x.MessagePart.GetBodyAsText();
                    }
                    else
                    {
                        var textBody =
                            x.MessagePart.MessageParts.FirstOrDefault(
                                p => p.ContentType.MediaType.Equals("text/plain", StringComparison.OrdinalIgnoreCase));
                        if (textBody != null)
                            textMessage = textBody.GetBodyAsText();

                        var htmlBody =
                            x.MessagePart.MessageParts.FirstOrDefault(
                                p => p.ContentType.MediaType.Equals("text/html", StringComparison.OrdinalIgnoreCase));
                        if (htmlBody != null)
                            htmlMessage = htmlBody.GetBodyAsText();
                    }
                    
                    ReminderRequestHandler.Build(x.Headers.From.Address, x.Headers.Subject, textMessage, htmlMessage, EndpointType.Email, "reminder@example.com", null);
                }
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
        }
    }
}