using System;

namespace ReminderHero.Models
{
    public static class RefundCalculator
    {
        public static int CalculateRefundInCents(DateTime paymentDate, DateTime currentDate, int paymentAmountInCents)
        {
            var hours= currentDate.Subtract(paymentDate).TotalHours;
            var amountToRefund = (int)Math.Round((paymentAmountInCents * (1 - (double)hours/ (30 * 24))));

            return amountToRefund;
        }
    }
}