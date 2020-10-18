using System;
using System.Collections.Generic;
using System.Linq;

namespace ReminderHero.Models.DataAccess
{
    public interface IReminderRepository
    {
        IEnumerable<Reminder> RemindersForCurrentMonth(User user, EndpointType type, DateTime utcNow);
        IEnumerable<Reminder> ActiveRecurringRemindersForUser(User user);
    }
    
    public class ReminderRepository : IReminderRepository
    {
        private readonly RemindersContext _context;

        public ReminderRepository(RemindersContext context)
        {
            _context = context;
        }

        public IEnumerable<Reminder> RemindersForCurrentMonth(User user, EndpointType type, DateTime utcNow)
        {
            var startDate = user.PlanStartDateUtc.HasValue ? user.PlanStartDateUtc.Value : DateTime.UtcNow;
            var dayOfMonthThatPlanStarted = startDate.Day;
            var currentDayOfMonth = utcNow.Day;
            DateTime dateToGoBackTo = new DateTime(utcNow.Year, utcNow.Month, 1);

            if (currentDayOfMonth > dayOfMonthThatPlanStarted)
            {
                // Currently 20th, Go back to 15th of this month.
                dateToGoBackTo = dateToGoBackTo.AddDays(dayOfMonthThatPlanStarted);
            }
            else
            {
               // Currently 10th. Go back to 15th of previous month
                dateToGoBackTo = dateToGoBackTo.AddMonths(-1).AddDays(dayOfMonthThatPlanStarted);
            }

            return
                _context.Reminders.Where(
                    r => r.UserId == user.UserId && r.Type == (int) type && r.CreatedDate > dateToGoBackTo).ToList();
        }

        public IEnumerable<Reminder> ActiveRecurringRemindersForUser(User user)
        {
            var reminders =
                _context.Reminders.Where(
                    r => r.UserId == user.UserId && r.RecurrenceType != (int) Recurrence.Once && !r.Handled);
            return reminders.ToList();
        }
    }
}
