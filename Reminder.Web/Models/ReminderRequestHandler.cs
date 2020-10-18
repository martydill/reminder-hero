using System;
using System.Linq;
using ReminderHero.Models;
using ReminderHero.Models.DataAccess;
using ReminderHero.Parser;
using ReminderHero.Web.Mailers;

namespace ReminderHero.Web.Models
{
    public static class ReminderRequestHandler
    {
        public static void Build(string address, string subject, string textMessage, string htmlMessage, EndpointType type, string to, Endpoint endpoint)
        {
            User user = null;

            try
            {
                using (var ctx = new RemindersContext())
                {
                    var strippedAddress = Endpoint.GenerateStrippedAddressFrom(address, EndpointType.Phone).ToLower();

                    if (endpoint == null)
                    {
                        endpoint = ctx.Endpoints.SingleOrDefault(
                            e =>
                            (e.Type == (int)EndpointType.Phone && (int)type == (int)EndpointType.Phone &&
                             e.StrippedAddress.ToLower().Equals(strippedAddress))
                            ||
                            (e.Type == (int)EndpointType.Email && (int)type == (int)EndpointType.Email &&
                             e.Address.ToLower().Equals(address.ToLower())));
                    }

                    if (endpoint != null)
                    {
                        user = ctx.Users.Single(u => u.UserId == endpoint.UserId);
                    }

                    var r = ReminderBuilder.Build(address, subject, textMessage, DateTime.Now, endpoint, user, new ReminderRepository(ctx));
                    r.To = to;
                    r.ReminderType = type;
                    ctx.Reminders.Add(r);
                    ctx.SaveChanges();

                    if (user != null && user.GoogleAccount != null)
                    {
                        var google = new GoogleService(user.GoogleAccount);
                        google.SendReminder(r);
                        ctx.SaveChanges();
                    }
                }
            }
            catch (ReminderFailureException e)
            {
                var planId = (user != null && user.PricePlanId != null) ? user.PricePlanId.Value : PricePlan.FreePlanId;
                var pricePlan = PricePlan.PlanForId(planId);

                string message = "";

                // Can't do it with the current plan

                if (e.Data0 == ReminderFailureReason.RecurringRemindersNotSupported)
                {
                    message = "Sorry, your current price plan doesn't support recurring reminders. To upgrade, visit https://example.com/Account/PricePlan";
                }
                else if (e.Data0 == ReminderFailureReason.SmsNotSupported)
                {
                     message = "Sorry, your current price plan doesn't support SMS reminders. To upgrade, visit https://example.com/Account/PricePlan";
                }
                else if (e.Data0 == ReminderFailureReason.OverReminderLimit)
                {
                    int limit = endpoint.EndpointType == EndpointType.Email
                                    ? pricePlan.EmailPerMonth
                                    : pricePlan.SmsPerMonth;
                    string word = endpoint.EndpointType == EndpointType.Email ? "email" : "SMS";

                    message =
                        String.Format(
                            "Sorry, you've reached your limit of {0} {1} reminders per month. To upgrade, visit https://example.com/Account/PricePlan", limit, word);
                }
                else if(e.Data0 == ReminderFailureReason.CouldntUnderstand)
                {
                    message = "Sorry, I didn't understand that reminder! Did you include both the what and the when?";
                }
                else if (e.Data0 == ReminderFailureReason.UnknownEndpoint)
                {
                    if(type == EndpointType.Email)
                    {
                        message = "Sorry, I didn't recognize the email address '" + address + "'. Have you added it in the My Email Addresses section of the website?"; 
                    }
                    else if(type == EndpointType.Phone)
                    {
                        message = "Sorry, I didn't recognize the number " + address + ". Have you added it in the My Phone Numbers section of the website?";
                    }
                }

                DeliverMessageTo(endpoint, type, address, message, to);
                Elmah.ErrorSignal.FromCurrentContext().Raise(e);
            }
            catch (Exception e)
            {
                if (type == EndpointType.Email)
                {
                    var m = new UserMailer();
                    m.Error(address, subject, textMessage).SendAsync();
                }

                Elmah.ErrorSignal.FromCurrentContext().Raise(e);
            }
        }

        private static void DeliverMessageTo(Endpoint endpoint, EndpointType endpointType, string address, string message, string to)
        {
            string addressToSendTo = endpoint != null ? endpoint.Address : address;

            if (endpointType == EndpointType.Email)
            {
                var m = new UserMailer();
                m.DisallowedByPlan(addressToSendTo, message).SendAsync();
            }
            else if (endpointType == EndpointType.Phone)
            {
                var x = new Twilio.TwilioRestClient("example", "example");
                x.SendSmsMessage(to, addressToSendTo, message);
            }
        }
    }
}