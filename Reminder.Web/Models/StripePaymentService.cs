using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ReminderHero.Models;
using Stripe;

namespace ReminderHero.Web.Models
{
    public class StripePaymentService 
    {
        private readonly RemindersContext _context;

        public StripePaymentService(RemindersContext context)
        {
            _context = context;
        }

        public void SetPricePlanForExistingUser(string creditCardToken, int pricePlanId, Guid userId, string couponCode)
        {
            var myUser = _context.Users.SingleOrDefault(u => u.UserId == userId);
            if (myUser == null)
                throw new ArgumentException("User with id " + userId + " not found");

            var pricePlan = PricePlan.PlanForId(pricePlanId);
            if (pricePlan == null)
                throw new InvalidOperationException("Price plan id " + pricePlanId + " not found");

            var customerService = new StripeCustomerService();

            if (pricePlan.Id == PricePlan.FreePlanId)
            {
                // switch to free plan - cancel subscription
                customerService.CancelSubscription(myUser.StripeCustomerId);

                var chargeService = new StripeChargeService();
                var lastCharge = chargeService.List(1, 0, myUser.StripeCustomerId).FirstOrDefault(); // most recent charge

                var refund = RefundCalculator.CalculateRefundInCents(lastCharge.Created, DateTime.UtcNow,
                                                                    lastCharge.AmountInCents.Value);
                chargeService.Refund(lastCharge.Id, refund);
            }
            else if(myUser.StripeCustomerId != null)
            {
                var myUpdatedSubscription = new StripeCustomerUpdateSubscriptionOptions();

                // Switch to (different) paid plan - update card if necessary, set plan
                if (creditCardToken != null)
                {
                    myUpdatedSubscription.TokenId = creditCardToken;
                }

                if (!String.IsNullOrEmpty(couponCode))
                    myUpdatedSubscription.CouponId = couponCode;

                myUpdatedSubscription.PlanId = pricePlan.PlanId;

                customerService.UpdateSubscription(myUser.StripeCustomerId, myUpdatedSubscription);
            }
            else
            {
                CreateCustomerForUser(creditCardToken, pricePlan.PlanId, myUser.UserId, couponCode);
            }
            myUser.PlanStartDateUtc = DateTime.Now.ToUniversalTime();
            myUser.PricePlanId = pricePlanId;

            _context.SaveChanges();
        }

        public void CreateCustomerForUser(string creditCardToken, string pricePlanName, Guid userId, string couponCode = null)
        {
            var myUser = _context.Users.SingleOrDefault(u => u.UserId == userId);
            if (myUser == null)
                throw new ArgumentException("User with id " + userId + " not found");

            var myCustomer = new StripeCustomerCreateOptions();
            myCustomer.Email = myUser.Email;
            if (!String.IsNullOrEmpty(creditCardToken))
            {
                myCustomer.PlanId = pricePlanName;
                myCustomer.TokenId = creditCardToken;
                myCustomer.CouponId = couponCode;
            }

            myCustomer.Description = myUser.Email;
            var customerService = new StripeCustomerService(); // todo - mock so that i can test this class

            PricePlan pricePlan = null;
            if (!String.IsNullOrEmpty(creditCardToken))
            {
                pricePlan = _context.PricePlans.SingleOrDefault(pp => pp.PlanId == pricePlanName);
                if (pricePlan == null)
                    throw new InvalidOperationException("Price plan " + pricePlanName + " not found");
            }

            StripeCustomer stripeCustomer = customerService.Create(myCustomer);

            myUser.StripeCustomerId = stripeCustomer.Id;

            if (!String.IsNullOrEmpty(myCustomer.PlanId))
            {
                myUser.PricePlanId = pricePlan.Id;
            }
            else
            {
                myUser.PricePlanId = PricePlan.FreePlanId;
            }

            myUser.PlanStartDateUtc = DateTime.UtcNow;
            _context.SaveChanges();
        }
    }
}