using System;
using NUnit.Framework;
using ReminderHero.Web.Models;
using Shouldly;

namespace ReminderHero.Data.Tests
{
    class RefundCalculatorTest
    {
        [Test]
        public void seven_day_refund_amount_correct()
        {
            var startDate = DateTime.Now.AddDays(-7);
            var currentDate = DateTime.Now;
            var amount = 1900;

            RefundCalculator.CalculateRefundInCents(startDate, currentDate, amount).ShouldBe(1457);
        }

        [Test]
        public void half_day_refund_amount_correct()
        {
            var startDate = DateTime.Now.AddHours(-12);
            var currentDate = DateTime.Now;
            var amount = 1900;

            RefundCalculator.CalculateRefundInCents(startDate, currentDate, amount).ShouldBe(1900 - 32);
        }
    }
}
