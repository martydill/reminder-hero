using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace ReminderHero.Models
{
    [Table("PhoneNumber")]
    public class PhoneNumber 
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Number { get; set; }

        public string Country { get; set; }
    }
}