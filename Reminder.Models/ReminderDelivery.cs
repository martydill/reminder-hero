using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReminderHero.Models
{
    [Table("ReminderDelivery")]
    public class ReminderDelivery
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("Reminder")]
        public int ReminderId { get; set; }

        public virtual Reminder Reminder { get; set; }

        public DateTime Date { get; set; }

        public string DeliveredTo { get; set; }

        public bool Success { get; set; }

        [ForeignKey("User")]
        public Guid? UserId { get; set; }

        public bool IsPreReminder { get; set; }

        public virtual User User { get; set; }
    }
}