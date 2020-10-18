using Mvc.Mailer;
using ReminderHero.Models;
using ReminderHero.Web.Controllers;
using ReminderHero.Web.Models;

namespace ReminderHero.Web.Mailers
{
    public interface IUserMailer
    {
        MvcMailMessage Reminder(Reminder reminder, HandleReminderType type);
        MvcMailMessage Error(string email, string subject, string message);
        MvcMailMessage Contact(ContactViewModel vm);
        MvcMailMessage DisallowedByPlan(string email, string message);
        MvcMailMessage Registered(string email, string plan);
    }
}