using System.Data.Entity;

namespace ReminderHero.Models
{
    public class RemindersContext : DbContext
    {
        public RemindersContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<PhoneNumber> PhoneNumbers { get; set; }

        public DbSet<Reminder> Reminders { get; set; }

        public DbSet<ReminderRequest> ReminderRequests { get; set; }

        public DbSet<ReminderDelivery> ReminderDeliveries { get; set; }

        public DbSet<GoogleCalendar> Calendars { get; set; }

        public DbSet<GoogleAccount> GoogleAccounts { get; set; }

        public DbSet<Endpoint> Endpoints { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PricePlan> PricePlans { get; set; }
    }
}
