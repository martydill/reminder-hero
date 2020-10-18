using System;
using System.Collections.Generic;
using AutoMapper;
using ReminderHero.Models;

namespace ReminderHero.Web.ViewModel
{
    public class BillingHistoryViewModel
    {
        public BillingHistoryViewModel(IEnumerable<Payment> payments)
        {
            Items = Mapper.Map<IEnumerable<Payment>, IEnumerable<PaymentViewModel>>(payments);
        }

        public IEnumerable<PaymentViewModel> Items { get; private set; }
    }

    public class PaymentViewModel
    {
        public string Description { get; set; }

        public decimal Amount { get; set; }

        public DateTime Date { get; set; }
    }
}