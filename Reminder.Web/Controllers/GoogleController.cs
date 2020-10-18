using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Newtonsoft.Json.Linq;
using ReminderHero.Models;
using ReminderHero.Models.DataAccess;
using Twilio.TwiML;
using Uri = System.Uri;

namespace ReminderHero.Web.Controllers
{
    public class GoogleController : Controller
    {
        //
        // GET: /Google/

        private static string UrlEncodeForGoogle(string url)
        {
            string UnReservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";
            var result = new StringBuilder();

            foreach (char symbol in url)
            {
                if (UnReservedChars.IndexOf(symbol) != -1)
                {
                    result.Append(symbol);
                }
                else
                {
                    result.Append('%' + String.Format("{0:X2}", (int)symbol));
                }
            }

            return result.ToString();
        }

        public ActionResult GCallback(string code, string error)
        {
            if(!String.IsNullOrEmpty(error))
            {
                return RedirectToAction("GoogleCalendar", "Account");
            }

            string baseUrl = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));

            string redirectUri = baseUrl + "google/gcallback";
            string url = String.Format("code={0}&client_id={1}&client_secret={2}&redirect_uri={3}&grant_type={4}",
                        code, GoogleService.ClientId, GoogleService.ClientSecret, redirectUri, "authorization_code");

            var request = HttpWebRequest.Create(GoogleAuthConsts.TokenUrl) as HttpWebRequest;
            string result = null;
            request.Method = "POST";
            request.KeepAlive = true;
            request.ContentType = "application/x-www-form-urlencoded";
            var bs = Encoding.UTF8.GetBytes(url);
            using (var reqStream = request.GetRequestStream())
            {
                reqStream.Write(bs, 0, bs.Length);
            }

            using (var response = request.GetResponse())
            {
                var sr = new StreamReader(response.GetResponseStream());
                result = sr.ReadToEnd();
                sr.Close();
            }

            dynamic responseJson = JObject.Parse(result);
            using (var context = new RemindersContext())
            {
                var user = context.Users.Single(u => u.Username == User.Identity.Name);
                user.GoogleAccount = new GoogleAccount();
                user.GoogleAccount.AccessToken = responseJson.access_token;
                user.GoogleAccount.RefreshToken = responseJson.refresh_token;
                user.GoogleAccount.TokenType = responseJson.token_type;
                int expirySeconds = responseJson.expires_in;
                user.GoogleAccount.ExpiryDate = DateTime.UtcNow.AddSeconds(expirySeconds);
                context.SaveChanges();

                var google = new GoogleService(user.GoogleAccount);

                var calendars = google.GetCalendars();
                if (calendars.Any())
                {
                    foreach (var calendar in calendars)
                    {
                        user.GoogleAccount.Calendars.Add(calendar);
                    }

                    var first = calendars.FirstOrDefault(cal => !cal.GoogleId.EndsWith("google.com"));
                    if (first == null)
                        first = calendars.FirstOrDefault();

                    first.SendEventsToMe = true;
                }

                context.SaveChanges();
            }

            var c = new MemoryCacheBase();
            c.Remove(User.Identity.Name);
            ThreadPool.QueueUserWorkItem(new WaitCallback(state => AddOutstandingTo(User.Identity.Name)));
            return RedirectToAction("GoogleCalendar", "Account");
        }

        private void AddOutstandingTo(string name)
        {
            using(var ctx = new RemindersContext())
            {
                var user = ctx.Users.Single(u => u.Username == name);

                var reminders = ctx.Reminders.Where(r => r.UserId == user.UserId && !r.Handled).ToList();
                var gs = new GoogleService(user.GoogleAccount);
                foreach(var r in reminders)
                {
                    gs.SendReminder(r);
                }
                ctx.SaveChanges();
            }
        }

        [HttpPost]
        public HttpStatusCodeResult Disconnect()
        {
            using (var ctx = new RemindersContext())
            {
                var user = ctx.Users.Single(u => u.Username == User.Identity.Name);
                if (user.GoogleAccount == null)
                    return new HttpStatusCodeResult(500);

                var reminders = ctx.Reminders.Where(r => r.UserId == user.UserId && !r.Handled).ToList();
                var calendars = user.GoogleAccount.Calendars.ToList();
                var gs = new GoogleService(user.GoogleAccount);
                foreach (var r in reminders)
                {
                    gs.DeleteReminder(r);
                    r.GoogleIds = null;
                }

                gs.Deauthorize();

                foreach (var cal in calendars)
                {
                    ctx.Calendars.Remove(cal);
                }
                ctx.GoogleAccounts.Remove(user.GoogleAccount);
                ctx.SaveChanges();
            }

            return new HttpStatusCodeResult(200);
        }

        [HttpPost]
        public RedirectResult ConnectToGoogle()
        {
            string baseUrl = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));
            var scopes = new List<string>();
            scopes.Add(CalendarService.Scope.Calendar);

            var authReq = new GoogleAuthorizationCodeRequestUrl(new Uri(GoogleAuthConsts.AuthorizationUrl))
            {
                RedirectUri = baseUrl + "google/gcallback",
                ClientId = GoogleService.ClientId,
                AccessType = "offline",
                Scope = string.Join(" ", scopes),
                ApprovalPrompt = "force"
            };
            
            return Redirect(authReq.Build().AbsoluteUri);
        }
    }
}
