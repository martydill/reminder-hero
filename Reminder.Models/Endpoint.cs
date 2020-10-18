using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace ReminderHero.Models
{
    public enum EndpointType
    {
        Email = 0,
        Phone = 1
    }

    [Table("Endpoint")]
    public class Endpoint
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Address { get; set; }

        public string StrippedAddress { get; set; }

        public decimal UtcOffset { get; set; }

        public int Type { get; set; }

        [ForeignKey("User")]
        public Guid UserId { get; set; }

        public virtual User User { get; set; }

        [NotMapped]
        public EndpointType EndpointType
        {
            get { return (EndpointType) Type; }
            set { Type = (int) value; }
        }

        public DateTime CreatedDateUtc { get; set; }

        public bool Enabled { get; set; }

        public static string GenerateStrippedAddressFrom(string address, EndpointType type)
        {
            if(type == EndpointType.Phone)
                return new String(address.ToCharArray().Where(c => Char.IsDigit(c)).ToArray());

            return address;
        }
    }
}