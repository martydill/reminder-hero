using System.Linq;
using System.Web.Security;
using ReminderHero.Models;

namespace ReminderHero.Models
{
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<RemindersContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(RemindersContext context)
        {
            //WebSecurity.Register("Demo", "123456", "demo@demo.com", true, "Demo", "Demo");
            Roles.CreateRole("Admin");
            Roles.AddUserToRole("Demo", "Admin");
            if (!context.PricePlans.Any())
            {
                var freePlan = new PricePlan() {Id = PricePlan.FreePlanId};
                freePlan.MonthlyCost = 0m;
                freePlan.Name = "Free";
                context.PricePlans.Add(freePlan);

                var premiumPlan = new PricePlan() {Id = PricePlan.PremiumPlanId};
                premiumPlan.MonthlyCost = PricePlan.PremiumPlanCost;
                premiumPlan.Name = "Premium";
                premiumPlan.PlanId = PricePlan.PremiumPlanStripeId;
                context.PricePlans.Add(premiumPlan);

                var businessPlan = new PricePlan() {Id = PricePlan.BusinessPlanId};
                businessPlan.Name = "Business";
                businessPlan.PlanId = PricePlan.BusinessPlanStripeId;
                businessPlan.MonthlyCost = PricePlan.BusinessPlanCost;
                context.PricePlans.Add(businessPlan);

                context.SaveChanges();
            }
        }
    }
}
