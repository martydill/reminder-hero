using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReminderHero.Models
{
    [Table("ReminderRequest")]
    public class ReminderRequest
    {
        public static readonly int Email = 1;
        public static readonly int Sms = 2;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public string RawText { get; set; }

        public byte[] RawBytes { get; set; }

        [Required]
        public int RequestType { get; set; }
    }
}