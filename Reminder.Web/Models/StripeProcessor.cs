using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using ReminderHero.Models;
using ReminderHero.Web.Mailers;
using Stripe;

namespace ReminderHero.Web.Models 
{
    public class StripeProcessor
    {
        private readonly RemindersContext _context;
        private readonly IUserMailer _mailSender;

        public StripeProcessor(RemindersContext context, IUserMailer mailSender)
        {
            _context = context;
            _mailSender = mailSender;
        }

        public void Process(string json)
        {
            var stripeEvent = StripeEventUtility.ParseEvent(json);
            if (stripeEvent == null)
                return;

            switch (stripeEvent.Type)
            {
                case "charge.refunded":
                case "charge.succeeded":
                    StripeCharge stripeCharge = Stripe.Mapper<StripeCharge>.MapFromJson(stripeEvent.Data.Object.ToString());
                    var user = _context.Users.SingleOrDefault(u => u.StripeCustomerId == stripeCharge.CustomerId);
                    if (user == null)
                        return;

                    var payment = new Payment();
                    payment.UserId = user.UserId;
                    if (stripeEvent.Type == "charge.refunded")
                        payment.Amount = 0 - Convert.ToDecimal(stripeCharge.AmountInCentsRefunded, CultureInfo.InvariantCulture) / 100;
                    else
                        payment.Amount = Convert.ToDecimal(stripeCharge.AmountInCents, CultureInfo.InvariantCulture) / 100;

                    payment.DateUtc = stripeCharge.Created;
                    payment.StripeChargeId = stripeCharge.Id;
                    payment.Description = "Monthly subscription fee " + GetPricePlan(user.PricePlanId, payment.Amount); // todo - plan name
                    payment.StripeEventType = stripeEvent.Type;

                    if (stripeEvent.Type == "charge.refunded")
                    {
                        payment.Description += " - refund";
                        // Refunds have same date as original transaction. Set to today's date instead.
                        payment.DateUtc = DateTime.UtcNow;
                       // payment.Amount = -payment.Amount;
                    }

                    _context.Payments.Add(payment);
                    _context.SaveChanges();
                    break;
            }
        }

        private string GetPricePlan(int? pricePlanId, decimal price)
        {
            if (pricePlanId == PricePlan.PremiumPlanId)
                return " - Premium Plan";
            else if (pricePlanId == PricePlan.BusinessPlanId)
                return " - Business Plan";
            else if (pricePlanId == PricePlan.EmailPlanId)
                return " - Email Plan";

            else // If plan has been canceled we won't have a price plan id
                if (price == PricePlan.PremiumPlanCost * 100)
                    return " - Premium Plan";
                else if (price == PricePlan.BusinessPlanCost * 100)
                    return " - Business Plan";
                else if (price == PricePlan.EmailPlanCost * 100)
                    return " - Email Plan";

            return "";
        }
    }

    public class InvoiceViewModel
    {
        public string PlanName { get; set; }

        public string PaymentAmount { get; set; }

        public string PaymentType { get; set; }

        public string Email { get; set; }
    }
}