using System;
using System.ComponentModel.DataAnnotations;

namespace ReminderHero.Models
{
    public class PricePlan
    {
        public static readonly int FreePlanEmailAddressesAllowed = 1;
        public static readonly int PremiumPlanEmailAddressesAllowed = 5;
        public static readonly int EmailPlanEmailAddressesAllowed = 2;
 
        public static readonly int FreePlanPhoneNumbersAllowed = 0;
        public static readonly int PremiumPlanPhoneNumbersAllowed = 5;
        public static readonly int EmailPlanPhoneNumbersAllowed = 0;

        public static readonly int FreePlanEmails = 5;
        public static readonly int PremiumPlanEmails = 100;
        public static readonly int EmailPlanEmails = 100;

        public static readonly int PremiumPlanSmses = 100;
        public static readonly int BusinessPlanSmses = 1000;

        public static readonly decimal EmailPlanCost = 4m;
        public static readonly decimal PremiumPlanCost = 9m;

        public static readonly decimal BusinessPlanCost = 49m;

        public static readonly int FreePlanId = 1;

        public static readonly string FreePlanName = "free-plan";

        public static readonly int PremiumPlanId = 2;

        public static readonly int BusinessPlanId = 3;

        public static readonly int EmailPlanId = 4;

        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public decimal MonthlyCost { get; set; }

        public string PlanId { get; set; }

        public int SmsPerMonth { get; set; }

        public int EmailPerMonth { get; set; }

        public bool RecurringReminders { get; set; }

        public static readonly string FreePlanStripeId = "free-plan";

        public static readonly string PremiumPlanStripeId = "premium-plan";

        public static readonly string BusinessPlanStripeId = "business-plan";

        public static readonly string EmailPlanStripeId = "email-plan";

        public int AllowedEmailAddresses { get; set; }

        public int AllowedPhoneNumbers { get; set; }

        public static string NameFor(int? planId)
        {
            if (planId == null || planId == FreePlanId)
                return "Free";
            else if (planId == PremiumPlanId)
                return "Premium";
            else if (planId == BusinessPlanId)
                return "Business";
            else if (planId == EmailPlanId)
                return "Email";

            throw new ArgumentException("Price plan id " + planId + " not found!");
        }

        public static PricePlan PlanForId(int? selectedPricePlanId)
        {
            int planId;
            if (selectedPricePlanId == null)
                planId = FreePlanId;
            else
                planId = selectedPricePlanId.Value;

            var pp = new PricePlan();
            pp.Name = NameFor(selectedPricePlanId);
            pp.Id = planId;
            pp.MonthlyCost = CostFor(planId);
            pp.PlanId = StripeIdFor(planId);
            if (planId == FreePlanId)
            {
                pp.SmsPerMonth = 0;
                pp.EmailPerMonth = FreePlanEmails;
                pp.RecurringReminders = false;
                pp.AllowedEmailAddresses = FreePlanEmailAddressesAllowed;
                pp.AllowedPhoneNumbers = FreePlanPhoneNumbersAllowed;
            }
            else if (planId == PremiumPlanId)
            {
                pp.SmsPerMonth = PremiumPlanSmses;
                pp.EmailPerMonth = PremiumPlanEmails;
                pp.RecurringReminders = true;
                pp.AllowedEmailAddresses = PremiumPlanEmailAddressesAllowed;
                pp.AllowedPhoneNumbers = PremiumPlanPhoneNumbersAllowed;
            }
            else if (planId == EmailPlanId)
            {
                pp.SmsPerMonth = 0;
                pp.EmailPerMonth = EmailPlanEmails;
                pp.RecurringReminders = true;
                pp.AllowedEmailAddresses = EmailPlanEmailAddressesAllowed;
                pp.AllowedPhoneNumbers = EmailPlanPhoneNumbersAllowed;
            }
            else
            {
                throw new Exception("Unknown plan " + planId);
            }

            return pp;
        }

        private static string StripeIdFor(int selectedPricePlanId)
        {
            if (selectedPricePlanId == PremiumPlanId)
                return PremiumPlanStripeId;
            else if (selectedPricePlanId == BusinessPlanId)
                return BusinessPlanStripeId;
            else if (selectedPricePlanId == EmailPlanId)
                return EmailPlanStripeId;
            else
                return String.Empty;
        }

        private static decimal CostFor(int planId)
        {
            if (planId == FreePlanId)
                return 0;
            else if (planId == PremiumPlanId)
                return PremiumPlanCost;
            else if (planId == BusinessPlanId)
                return BusinessPlanCost;
            else if (planId == EmailPlanId)
                return EmailPlanCost;

            throw new ArgumentException("Price plan id " + planId + " not found!");
        }
    }
}
