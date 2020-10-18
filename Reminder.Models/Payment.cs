using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReminderHero.Models
{
    [Table("Payment")]
    public class Payment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public Guid UserId { get; set; }

        public DateTime DateUtc { get; set; }

        public string StripeInvoiceId { get; set; }

        public string StripePaymentId { get; set; }

        public string StripeChargeId { get; set; }

        public string StripeEventType { get; set; }

        public virtual User User { get; set; }
    }
}
