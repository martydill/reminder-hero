using System.Collections.Generic;
using ReminderHero.Models;

namespace ReminderHero.Web.ViewModel
{
    public class PricePlanViewModel
    {
        public string Email { get; set; }

        public PricePlanViewModel(int selectedPricePlanId, string email, bool isBetaUser)
        {
            BetaDiscountAmount = isBetaUser ? " (25% Beta Discount!)" : "";
            DiscountMultiplier = isBetaUser ? .75m : 1;
            Email = email;
            var pricePlan = PricePlan.PlanForId(selectedPricePlanId);
            CurrentPricePlanName = pricePlan.Name;
            MonthlyCost = pricePlan.MonthlyCost * DiscountMultiplier;

            CanCancel = pricePlan.MonthlyCost > 0;

            //PlansToSwitchTo = Mapper.Map<IEnumerable<PricePlan>, IEnumerable<PlanToSwitchTo>>(allPricePlans.Where(p => p.MonthlyCost > 0 && p.Id != selectedPricePlanId));
        }

        public string BetaDiscountAmount { get; set; }

        public decimal DiscountMultiplier { get; set; }

        public string CurrentPricePlanName { get; set; }

        public decimal MonthlyCost { get; set; }

        public bool CanCancel { get; set; }

        public IEnumerable<PlanToSwitchTo> PlansToSwitchTo { get; set; }

        public bool GetsBetaUserDiscount { get; set; }
    }

    public class PlanToSwitchTo
    {
        public string Name { get; set; }

        public decimal MonthlyCost { get; set; }

        public int Id { get; set; }
    }
}