using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ReminderHero.Models;
using Shouldly;

namespace ReminderHero.Data.Tests
{
    public class FindRemindersToHandleTests 
    {
        [Test]
        public void supports_reminders_without_users()
        {
            var reminders = new List<Reminder>();
            var r = new Reminder();
            reminders.Add(r);
            DateTime date = new DateTime(2013, 08, 10, 2, 30, 0);
            r.ReminderDate = date;

            var remindersToHandle = Reminder.FindRemindersToHandle(new FakeDbSet<Reminder>(reminders), date.AddMinutes(1));
            remindersToHandle.Count().ShouldBe(1);
            remindersToHandle.First().Item1.ShouldBe(HandleReminderType.Real);
        }

        [Test]
        public void find_due_reminders()
        {
            var reminders = new List<Reminder>();
            var r = new Reminder();
            r.User = new User();
            reminders.Add(r);
            DateTime date = new DateTime(2013, 08, 10, 2, 30, 0);
            r.ReminderDate = date;

            var remindersToHandle = Reminder.FindRemindersToHandle(new FakeDbSet<Reminder>(reminders), date.AddMinutes(1));
            remindersToHandle.Count().ShouldBe(1);
            remindersToHandle.First().Item1.ShouldBe(HandleReminderType.Real);
        }

        [Test]
        public void find_reminder_for_minutes_away_at_time()
        {
            var reminders = new List<Reminder>();
            var r = new Reminder();
            r.User = new User() {Reminder1Number = 15, Reminder1Enabled = true};
            reminders.Add(r);
            var date = new DateTime(2013, 08, 10, 2, 30, 0);
            r.ReminderDate = date;

            var remindersToHandle = Reminder.FindRemindersToHandle(new FakeDbSet<Reminder>(reminders), date.AddMinutes(-14));
            remindersToHandle.Count().ShouldBe(1);
            remindersToHandle.First().Item1.ShouldBe(HandleReminderType.One);
        }

        [Test]
        public void find_reminder_for_reminder_2()
        {
            var reminders = new List<Reminder>();
            var r = new Reminder();
            r.User = new User() { Reminder2Number = 4 * 60, Reminder2Enabled = true };
            reminders.Add(r);
            var date = new DateTime(2013, 08, 10, 2, 30, 0);
            r.ReminderDate = date;

            var remindersToHandle = Reminder.FindRemindersToHandle(new FakeDbSet<Reminder>(reminders), date.AddMinutes(-3 * 60));
            remindersToHandle.Count().ShouldBe(1);
            remindersToHandle.First().Item1.ShouldBe(HandleReminderType.Two);
        }

        [Test]
        public void find_reminder_for_reminder_3()
        {
            var reminders = new List<Reminder>();
            var r = new Reminder();
            r.User = new User() { Reminder3Number = 48 * 60, Reminder3Enabled = true };
            reminders.Add(r);
            var date = new DateTime(2013, 08, 10, 2, 30, 0);
            r.ReminderDate = date;

            var remindersToHandle = Reminder.FindRemindersToHandle(new FakeDbSet<Reminder>(reminders), date.AddMinutes(-47 * 60));
            remindersToHandle.Count().ShouldBe(1);
            remindersToHandle.First().Item1.ShouldBe(HandleReminderType.Three);
        }

        [Test]
        public void find_reminder_for_reminder_4()
        {
            var reminders = new List<Reminder>();
            var r = new Reminder();
            r.User = new User() { Reminder4Number = 11 * 24 * 60, Reminder4Enabled = true };
            reminders.Add(r);
            var date = new DateTime(2013, 08, 10, 2, 30, 0);
            r.ReminderDate = date;

            var remindersToHandle = Reminder.FindRemindersToHandle(new FakeDbSet<Reminder>(reminders), date.AddMinutes(-10 * 24 * 60));
            remindersToHandle.Count().ShouldBe(1);
            remindersToHandle.First().Item1.ShouldBe(HandleReminderType.Four);
        }

        [Test]
        public void nothing_found_when_reminders_disabled()
        {
            var reminders = new List<Reminder>();
            var r = new Reminder();
            r.User = new User() { Reminder4Number = 11 * 24 * 60, Reminder3Number = 23456, Reminder2Number = 234, Reminder1Number = 5 };
            reminders.Add(r);
            var date = new DateTime(2013, 08, 10, 2, 30, 0);
            r.ReminderDate = date;

            var remindersToHandle = Reminder.FindRemindersToHandle(new FakeDbSet<Reminder>(reminders), date.AddMinutes(-0));
            remindersToHandle.Count().ShouldBe(0);
        }

        [Test]
        public void nothing_found_when_reminders_already_fired()
        {
            var reminders = new List<Reminder>();
            var r = new Reminder() { Reminder1Fired = true, Reminder2Fired =  true, Reminder3Fired = true, Reminder4Fired = true};
            r.User = new User() { Reminder4Number = 11 * 24 * 60, Reminder3Number = 23456, Reminder2Number = 234, Reminder1Number = 5
            ,Reminder1Enabled = true, Reminder2Enabled = true,Reminder3Enabled = true,Reminder4Enabled = true};
            reminders.Add(r);
            var date = new DateTime(2013, 08, 10, 2, 30, 0);
            r.ReminderDate = date;

            var remindersToHandle = Reminder.FindRemindersToHandle(new FakeDbSet<Reminder>(reminders), date.AddMinutes(-0));
            remindersToHandle.Count().ShouldBe(0);
        }
    }

    public class FakeDbSet<T> : System.Data.Entity.IDbSet<T> where T : class
    {
        private readonly List<T> _list = new List<T>();

        public FakeDbSet()
        {
            _list = new List<T>();
        }

        public FakeDbSet(IEnumerable<T> contents)
        {
            _list = contents.ToList();
        }

        #region IDbSet<T> Members

        public T Add(T entity)
        {
            _list.Add(entity);
            return entity;
        }

        public T Attach(T entity)
        {
            this._list.Add(entity);
            return entity;
        }

        public TDerivedEntity Create<TDerivedEntity>() where TDerivedEntity : class, T
        {
            throw new NotImplementedException();
        }

        public T Create()
        {
            throw new NotImplementedException();
        }

        public T Find(params object[] keyValues)
        {
            throw new NotImplementedException();
        }

        public System.Collections.ObjectModel.ObservableCollection<T> Local
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public T Remove(T entity)
        {
            this._list.Remove(entity);
            return entity;
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        #endregion

        #region IQueryable Members

        public Type ElementType
        {
            get { return this._list.AsQueryable().ElementType; }
        }

        public System.Linq.Expressions.Expression Expression
        {
            get { return this._list.AsQueryable().Expression; }
        }

        public IQueryProvider Provider
        {
            get { return this._list.AsQueryable().Provider; }
        }

        #endregion
    }
}
