using System.Collections.Generic;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Routing;

namespace ReminderHero.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("TOS", "TermsOfService", new { controller = "Home", action = "TermsOfService" });
            routes.MapRoute("PP", "PrivacyPolicy", new { controller = "Home", action = "PrivacyPolicy" });

            var landingPages = new List<string>()
                {
                    "appointment-reminders",
                    "birthday-reminders",
                    "anniversary-reminders",
                    "meeting-reminders",
                    "event-reminders",
                    "tv-show-reminders",
                    "task-reminders",
                    "bill-reminders",
                    "date-reminders",
                    "birth-control-reminders",
                    "pill-reminders",
                    "schedule-reminders",
                    "homework-reminders",
                    "what-can-reminder-hero-do"
                };

            foreach (var landingPage in landingPages)
            {
                string pageName = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(landingPage).Replace("-", "");
                routes.MapRoute("route-" + landingPage, landingPage, new {controller = "Home", action = pageName});
            }

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}