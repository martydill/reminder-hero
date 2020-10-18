using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using ReminderHero.Models.Utility;

namespace ReminderHero.Models.DataAccess
{
    public interface IGoogleService
    {
        void SendReminder(Reminder reminder);

        IEnumerable<GoogleCalendar> GetCalendars();

        void Deauthorize();
    }

    public class GoogleService : IGoogleService
    {
        private readonly GoogleAccount _account;
        public static readonly string ClientId = "google-client-id-example";
        public static readonly string ClientSecret = "google-client-secret-example";

        public GoogleService(GoogleAccount account)
        {
            _account = account;
            if (account == null)
                throw new ArgumentNullException("account");
        }

        public void SendReminder(Reminder reminder)
        {
            if(reminder == null)
                throw new ArgumentNullException("reminder");

            var service = GetService();

            if (String.IsNullOrEmpty(reminder.GoogleIds))
            {
                string googleids = "";
                foreach (var cal in _account.Calendars.Where(c => c.SendEventsToMe).ToList())
                {
                    var e = new Event();
                    e.Summary = reminder.Description;
                    var utcStart = new DateTime(reminder.ReminderDate.Ticks, DateTimeKind.Utc);
                    var utcEnd = new DateTime(reminder.ReminderDate.AddHours(1).Ticks, DateTimeKind.Utc);

                    e.Start = new EventDateTime {DateTime = utcStart, TimeZone = "UTC"};
                    e.End = new EventDateTime {DateTime = utcEnd, TimeZone = "UTC"};
                    if (reminder.Recurrence != Recurrence.Once)
                    {
                        e.Recurrence = new List<string>();
                        e.Recurrence.Add(Rfc2445Creator.From(reminder));
                    }
                    var inserted = service.Events.Insert(e, cal.GoogleId);
                    var result = inserted.Execute();
                    googleids += cal.GoogleId + "~" + result.Id + ":";
                }
                reminder.GoogleIds = googleids;
            }
        }

        public void DeleteReminder(Reminder reminder)
        {
            if (reminder == null)
                throw new ArgumentNullException("reminder");

            if (!String.IsNullOrEmpty(reminder.GoogleIds))
            {
                var parts = reminder.GoogleIds.Split(new char[]{':'}, StringSplitOptions.RemoveEmptyEntries);
                var service = GetService();
                foreach (var part in parts)
                {
                    var ids = part.Split('~');
                    var calendarId = ids[0];
                    var eventId = ids[1];
                    try
                    {
                        service.Events.Delete(calendarId, eventId).Execute();
                    }
                    catch(Exception e)
                    {
                        // Could've already been deleted
                    }
                }
            }
        }

        public IEnumerable<GoogleCalendar> GetCalendars()
        {
            var service = GetService();
            var events = service.CalendarList.List();
            var y = events.Execute();

            var calendars = new List<GoogleCalendar>();

            foreach (var calendar in y.Items)
            {
                var cal = new GoogleCalendar();
                cal.SendEventsToMe = false;
                cal.Summary = calendar.Summary;
                cal.GoogleId = calendar.Id;
                calendars.Add(cal);
            }
            return calendars;
        }
        
        public void Deauthorize()
        {
            var revocationRequestString = String.Format("https://accounts.google.com/o/oauth2/revoke?token={0}",
                _account.AccessToken);
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, revocationRequestString);

            var svc = GetService();
            svc.HttpClient.SendAsync(httpRequestMessage).Wait();
        }

        private CalendarService GetService()
        {
            var token = new TokenResponse
            {
                AccessToken = _account.AccessToken,
                RefreshToken = _account.RefreshToken
            };

            var credentials = new UserCredential(new GoogleAuthorizationCodeFlow(
                new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets =
                        new ClientSecrets() {ClientId = GoogleService.ClientId, ClientSecret = GoogleService.ClientSecret}
                }), "user", token);


            // Create the service.
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credentials,
                ApplicationName = "Reminder Hero",
            });
            
            return service;
        }


    }
}
