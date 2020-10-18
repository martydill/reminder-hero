using System;
using Mvc.Mailer;
using ReminderHero.Models;
using ReminderHero.Web.Controllers;
using ReminderHero.Web.Models;

namespace ReminderHero.Web.Mailers
{
    public class UserMailer : MailerBase, IUserMailer
    {
        public UserMailer()
        {
            MasterName = "_Layout";
        }

        public virtual MvcMailMessage Reminder(Reminder reminder, HandleReminderType type)
        {
            ViewBag.Reminder = reminder; 
            ViewBag.HandleType = type;

            return Populate(x =>
            {
                x.Subject = "Reminder from Reminder Hero!";
                x.ViewName = "Reminder";
                x.ReplyToList.Add("contact@example.com");
                x.To.Add(reminder.Email);
            });
        }

        public virtual MvcMailMessage Error(string email, string subject, string message)
        {
            ViewBag.Message = String.IsNullOrEmpty(message) ? subject : message;
            return Populate(x =>
                {
                    x.Subject = "Sorry, Reminder Hero couldn't create your reminder!";
                    x.ViewName = "Error";
                    x.ReplyToList.Add("contact@example.com");
                    x.To.Add(email);
                });
        }

        public virtual MvcMailMessage DisallowedByPlan(string email, string message)
        {
            ViewBag.Message = message;
            return Populate(x =>
            {
                x.Subject = "Sorry, Reminder Hero couldn't create your reminder!";
                x.ViewName = "Disallowed";
                x.ReplyToList.Add("contact@example.com");
                x.To.Add(email);
            });
        }

        public MvcMailMessage Contact(ContactViewModel vm)
        {
            ViewData.Model = vm;
            return Populate(x =>
            {
                x.Subject = "Reminder Hero Website Contact";
                x.ViewName = "Contact";
                x.To.Add("contact@example.com");
                x.From = new System.Net.Mail.MailAddress(vm.Email);
            });
        }

        public virtual MvcMailMessage Registered(string email, string plan)
        {
            ViewBag.Email = email;
            ViewBag.Plan = plan;
            return Populate(x =>
                {
                    x.From = new System.Net.Mail.MailAddress("contact@example.com", "Reminder Hero");
                    x.Subject = plan + " Registration from " + email;
                    x.ViewName = "Registered";
                    x.To.Add("contact@example.com");
                });
        }
    }
}