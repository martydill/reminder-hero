
using System.ComponentModel.DataAnnotations;

namespace ReminderHero.Web.Controllers
{
    public class ContactViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Email { get; set; }

        public string Phone { get; set; }

        public string Company { get; set; }

        [Required]
        public string Message { get; set; }

        public string LeaveBlank { get; set; }
    }
}