using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Calendar.v3;
using Google.Apis.Util.Store;
using MVCCentral.Framework;
using ReminderHero.Models;
using ReminderHero.Models.DataAccess;
using ReminderHero.Web.Mailers;
using ReminderHero.Web.Models;
using ReminderHero.Web.ViewModel;
using ReminderHero.Models.Utility;
using SecurityGuard.Core;
using SecurityGuard.Interfaces;
using SecurityGuard.Services;
using SecurityGuard.ViewModels;
using viewModels = ReminderHero.Web.Areas.SecurityGuard.ViewModels;

namespace ReminderHero.Web.Controllers
{
    [NonLocalRequireHttps]
    public class AccountController : BaseController
    {
        #region ctors

        private IMembershipService membershipService;
        private IAuthenticationService authenticationService;

        public AccountController()
        {
            this.membershipService = new MembershipService(Membership.Provider);
            this.authenticationService = new AuthenticationService(membershipService, new FormsAuthenticationService());
        }

        #endregion
        [Authorize]
        [ActionName("Welcome-Email")]
        public ActionResult WelcomeEmail()
        {
            return View();
        }

        [Authorize]
        [ActionName("Welcome-Free")]
        public ActionResult WelcomeFree()
        {
            return View();
        }

        [Authorize]
        [ActionName("Welcome-Premium")]
        public ActionResult WelcomePremium()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public ActionResult Reminders()
        {
            using (var context = new RemindersContext())
            {
                var currentUser = GetUser(context);
                var reminders = context.Reminders.Where(r => !r.Handled && r.UserId == currentUser.UserId);
                var vm = new RemindersViewModel() { Reminders = reminders.ToList() };
                foreach (var reminder in vm.Reminders)
                {
                    reminder.TimeZone = currentUser.TimeZone;
                }

                return View(vm);
            }
        }

        public ActionResult ContactSuccess()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Contact()
        {
            return View(new ContactViewModel());
        }

        [HttpPost]
        public ActionResult Contact(ContactViewModel vm)
        {
            new UserMailer().Contact(vm).SendAsync();
            return RedirectToAction("ContactSuccess");
        }

        [Authorize]
        [HttpGet]
        public ActionResult ReminderHistory()
        {
            using (var context = new RemindersContext())
            {
                var currentUser = GetUser(context);

                var histories = from r in context.Reminders
                                join rd in context.ReminderDeliveries
                                    on r.Id equals rd.ReminderId
                                join u in context.Users
                                    on r.UserId equals u.UserId
                                where r.UserId == currentUser.UserId
                                    && !rd.IsPreReminder
                                orderby rd.Date
                                select new ReminderHistoryDisplay
                                    {
                                        Description = r.Description,
                                        Address = rd.DeliveredTo,
                                        ReminderDate = rd.Date,
                                        RecurrenceInt = r.RecurrenceType,
                                        TimeZone = u.TimeZone
                                    };

                var vm = new ReminderHistoryViewModel() { Reminders = histories.ToList() };
                return View(vm);
            }
        }

        #region LogOn Methods

        [HttpGet]
        public virtual ActionResult LogOn()
        {
            var viewModel = new LogOnViewModel()
            {
                EnablePasswordReset = membershipService.EnablePasswordReset
            };
            return View(viewModel);
        }

        [HttpPost]
        public virtual ActionResult LogOn(LogOnViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (authenticationService.LogOn(model.UserName, model.Password, model.RememberMe))
                {
                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Reminders", "Account");
                    }
                }
                else
                {
                    MembershipUser user = membershipService.GetUser(model.UserName);
                    if (user == null)
                    {
                        ModelState.AddModelError("", "This account does not exist. Please try again.");
                    }
                    else
                    {
                        if (!user.IsApproved)
                        {
                            ModelState.AddModelError("", "Your account has not been approved yet.");
                        }
                        else if (user.IsLockedOut)
                        {
                            ModelState.AddModelError("", "Your account is currently locked.");
                        }
                        else
                        {
                            ModelState.AddModelError("", "The user name or password provided is incorrect.");
                        }
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return RedirectToAction("LogOn");
        }

        [HttpGet]
        public virtual ActionResult LogOff()
        {
            authenticationService.LogOff();

            return RedirectToAction("Index", "Home");
        } 
        #endregion

        #region Register Methods

        public virtual ActionResult Register(string plan)
        {
            var model = new viewModels.RegisterViewModel()
            {
                RequireSecretQuestionAndAnswer = membershipService.RequiresQuestionAndAnswer,
                Plan = plan
            };
            return View(model);
        }

        [HttpPost]
        public virtual ActionResult Register(viewModels.RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                MembershipCreateStatus createStatus;
                var user = Membership.CreateUser(model.UserName, model.Password, model.Email, model.SecretQuestion, model.SecretAnswer, true, out createStatus);
                
                if (createStatus == MembershipCreateStatus.Success)
                {
                    FormsAuthentication.SetAuthCookie(model.UserName, false /* createPersistentCookie */);
                 
                    using (var context = new RemindersContext())
                    {
                        var address = new Endpoint()
                            {
                                CreatedDateUtc = DateTime.UtcNow,
                                Enabled = true,
                                EndpointType = EndpointType.Email,
                                Address = model.Email,
                                UserId = new Guid(user.ProviderUserKey.ToString())
                            };
                        address.StrippedAddress = Endpoint.GenerateStrippedAddressFrom(address.Address, EndpointType.Email);
                        address.UtcOffset = -7; // leave this here?
                        context.Endpoints.Add(address);

                        var id = new Guid(user.ProviderUserKey.ToString());
                        var createdUser = context.Users.Single(u => u.UserId == id);
                        createdUser.TimeZone = model.Timezone;
                        context.SaveChanges();

                        var remindersForUser = context.Reminders.Where(r => r.Email == model.Email && r.UserId == null);
                        foreach (var reminder in remindersForUser)
                            reminder.UserId = id;

                        var deliveriesForUser = context.ReminderDeliveries.Where(r => r.DeliveredTo == model.Email);
                        foreach (var delivery in deliveriesForUser)
                            delivery.UserId = id;

                        var ps = new StripePaymentService(context);
                        ps.CreateCustomerForUser(model.StripeToken, model.Plan, id);
                        context.SaveChanges();
                    }

                    new UserMailer().Registered(model.Email, model.Plan).Send();

                    if (model.Plan == ReminderHero.Models.PricePlan.PremiumPlanStripeId)
                        return RedirectToAction("Welcome-Premium", "Account");
                    else if (model.Plan == ReminderHero.Models.PricePlan.EmailPlanStripeId)
                        return Redirect("Welcome-Email");
                    else
                        return RedirectToAction("Welcome-Free", "Account");
                }
                else
                {
                    ModelState.AddModelError("", ErrorCodeToString(createStatus));
                }
            }

            return RedirectToAction("Register");
        }


        #endregion

        #region ChangePassword Methods
        
        /// <summary>
        /// This allows the logged on user to change his password.
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public virtual ActionResult ChangePassword()
        {
            var viewModel = new ChangePasswordViewModel();

            return View(viewModel);
        }

        [Authorize]
        [HttpPost]
        public virtual ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // ChangePassword will throw an exception rather
                // than return false in certain failure scenarios.
                bool changePasswordSucceeded;
                try
                {
                    MembershipUser currentUser = Membership.GetUser(User.Identity.Name, false);
                    changePasswordSucceeded = currentUser.ChangePassword(model.OldPassword, model.NewPassword);
                }
                catch (Exception)
                {
                    changePasswordSucceeded = false;
                }

                if (changePasswordSucceeded)
                {
                    return RedirectToAction("ChangePasswordSuccess");
                }
                else
                {
                    ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                }
            }

            // If we got this far, something failed, redisplay form
            return RedirectToAction("ChangePassword");
        }

        //
        // GET: /Account/ChangePasswordSuccess

        public virtual ActionResult ChangePasswordSuccess()
        {
            return View();
        }


        #endregion

        #region Forgot Password Methods

        [HttpGet]
        public ActionResult ResetPassword(string token)
        {
            using (var c = new RemindersContext())
            {
                var user = c.Users.SingleOrDefault(u => u.PasswordVerificationToken == token);
                if (user == null || user.PasswordVerificationTokenExpirationDate < DateTime.UtcNow)
                {
                    ModelState.AddModelError("", "Sorry, it looks like that password reset link isn't valid");
                    return View();
                }

                var vm = new ChangePasswordViewModel() { PasswordResetToken = token };
                return View(vm);
            }
        }

        [HttpPost]
        public ActionResult ResetPassword(ChangePasswordViewModel vm)
        {
            using (var c = new RemindersContext())
            {
                var user = c.Users.SingleOrDefault(u => u.PasswordVerificationToken == vm.PasswordResetToken);
                if (user == null || user.PasswordVerificationTokenExpirationDate < DateTime.UtcNow)
                {
                    ModelState.AddModelError("", "Sorry, it looks like that password reset link isn't valid");
                    return View();
                }

                MembershipUser mu = membershipService.GetUser(user.Username);
                if (mu == null)
                {
                    ModelState.AddModelError("", "Sorry, that account couldn't be found");
                    return View(vm);
                }

                var old = mu.ResetPassword();
                if (mu.ChangePassword(old, vm.NewPassword))
                {
                    user.PasswordVerificationToken = null;
                    user.PasswordVerificationTokenExpirationDate = null;
                    c.SaveChanges();

                    if (WebSecurity.Login(mu.UserName, vm.NewPassword))
                        return RedirectToAction("Reminders");
                }

                return View(vm);
            }
        }
        /// <summary>
        /// This allows the non-logged on user to have his password
        /// reset and emailed to him.
        /// </summary>
        /// <returns></returns>
        public ActionResult ForgotPassword()
        {
            var viewModel = new ForgotPasswordViewModel()
            {
                RequiresQuestionAndAnswer = membershipService.RequiresQuestionAndAnswer
            };
            return View(viewModel);
        }

        /// <summary>
        /// Reset the password for the user and email it to him.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            // Get the userName by the email address
            string userName = membershipService.GetUserNameByEmail(model.Email);

            if (string.IsNullOrEmpty(userName))
            {
                ModelState.AddModelError("", "Sorry, I couldn't find an account with that email address");
                return RedirectToAction("ForgotPassword");
            }

            // Get the user by the userName
            MembershipUser user = membershipService.GetUser(userName);

            // Now reset the password
            string newPassword = string.Empty;

            using (var c = new RemindersContext())
            {
                var date = DateTime.UtcNow.AddHours(8);
                var token = PasswordGenerator.GeneratePassword(32);
                var userId = new Guid(user.ProviderUserKey.ToString());

                var dbUser = c.Users.Single(u => u.UserId == userId);
                dbUser.PasswordVerificationToken = token;
                dbUser.PasswordVerificationTokenExpirationDate = date;
                c.SaveChanges();

                string link = String.Format("https://example.com/Account/ResetPassword?token={0}", token);

            try
            {
                    string body = BuildMessageBody(user.UserName, newPassword, link, ConfigSettings.SecurityGuardEmailTemplatePath);
                Mail(model.Email, ConfigSettings.SecurityGuardEmailFrom, ConfigSettings.SecurityGuardEmailSubject, body, true);
            }
            catch (Exception)
            {
            }

            }
            return RedirectToAction("ForgotPasswordSuccess");
        }

        public ActionResult ForgotPasswordSuccess()
        {
            return View();
        }


        #endregion

        #region Status Codes
        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that email address already exists. Please enter a different email address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The email address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion

        #region Mailer Helpers

        /// <summary>
        /// This method encapsulates the email function.
        /// </summary>
        /// <param name="emailTo"></param>
        /// <param name="emailFrom"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="isHtml"></param>
        private void Mail(string emailTo, string emailFrom, string subject, string body, bool isHtml)
        {
            Email email = new Email();
            email.ToList = emailTo;
            email.FromEmail = emailFrom;
            email.Subject = subject;
            email.MessageBody = body;
            email.isHTML = isHtml;
            
            email.SendEmail(email);
        }

        /// <summary>
        /// This function builds the email message body from the ResetPassword.html file.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private string BuildMessageBody(string userName, string password, string url, string filePath)
        {
            string body = string.Empty;

            FileInfo fi = new FileInfo(Server.MapPath(filePath));
            string text = string.Empty;

            if (fi.Exists)
            {
                using (StreamReader sr = fi.OpenText())
                {
                    text = sr.ReadToEnd();
                }
                text = text.Replace("%UserName%", userName);
                text = text.Replace("%Password%", password);
                text = text.Replace("%Link%", url);
            }
            body = text;

            return body;
        }

        #endregion

        [Authorize]
        [HttpGet]
        public ActionResult NumbersToText()
        {
            using (var context = new RemindersContext())
            {
                var numbers = context.PhoneNumbers.ToList();
                return View(new NumbersToTextViewModel() { Numbers = numbers });
            }
        }

        [Authorize]
        public ActionResult EmailAddresses()
        {
            using (var context = new RemindersContext())
            {
                var currentUser = GetUser(context);
                var pricePlan = ReminderHero.Models.PricePlan.PlanForId(currentUser.PricePlanId);
                var endpoints = context.Endpoints.Where(e => e.UserId == currentUser.UserId && e.Type == (int)EndpointType.Email).ToList();

                var vm = new EndpointsViewModel() { EndpointType = EndpointType.Email, Endpoints = endpoints, CanUseAtAll = true, CanAddMore = endpoints.Count < pricePlan.AllowedEmailAddresses};
                return View(vm);
            }
        }

        [Authorize]
        public ActionResult PhoneNumbers()
        {
            using (var context = new RemindersContext())
            {
                var currentUser = GetUser(context);
                var pricePlan = ReminderHero.Models.PricePlan.PlanForId(currentUser.PricePlanId);
                var endpoints = context.Endpoints.Where(e => e.UserId == currentUser.UserId && e.Type == (int) EndpointType.Phone).ToList();
                var vm = new EndpointsViewModel() { EndpointType = EndpointType.Phone, Endpoints = endpoints, CanUseAtAll = pricePlan.SmsPerMonth > 0, CanAddMore = endpoints.Count < pricePlan.AllowedPhoneNumbers};
                return View(vm);
            }
        }

        private User GetUser(RemindersContext remindersContext)
        {
            var cache = new MemoryCacheBase();
            var name = User.Identity.Name;
            var currentUser = cache.CachedRead<User>(name, () => remindersContext.Users.SingleOrDefault(u => u.Username == name));
            return currentUser;
        }

        [Authorize]
        public ActionResult BillingHistory()
        {
            // todo - fix paging
            using (var context = new RemindersContext())
            {
                var currentUser = GetUser(context);
                var payments = context.Payments.Where(p => p.UserId == currentUser.UserId);
                var vm = new BillingHistoryViewModel(payments);
                return View(vm);
            }
        }

        [Authorize]
        public ActionResult PricePlan()
        {
            using (var context = new RemindersContext())
            {
                var user = GetUser(context);
                var vm = new PricePlanViewModel(user.PricePlanId.Value, user.Email, user.IsBetaUser);
                return View(vm);
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult SwitchToPricePlan(FormCollection fc)
        {
            string token = fc["StripeToken"];
            int newPlanId = Convert.ToInt32(fc["PricePlanId"]); 

            // todo - do i have to do this?
            //var pricePlan = _pricePlanRepository.GetPricePlanById(planId);
            //if (pricePlan == null)
            //    throw new ArgumentException("Price plan " + planId + " not found ");
            using (var ctx = new RemindersContext())
            {
                var user = GetUser(ctx);

                //user.PricePlanId = planId;
                //_userRepository.SaveUser(user);
                string couponCode = user.IsBetaUser ? "beta-user-coupon" : null;
                new StripePaymentService(ctx).SetPricePlanForExistingUser(token, newPlanId, user.UserId, couponCode);

                // Flush cache
                var cache = new MemoryCacheBase();
                cache.Remove(user.Username);

                if (newPlanId == ReminderHero.Models.PricePlan.FreePlanId)
                {
                    var repo = new ReminderRepository(ctx);

                    var recurringRemindersForUser = repo.ActiveRecurringRemindersForUser(user);
                    foreach (var recurringReminder in recurringRemindersForUser)
                    {
                        recurringReminder.Handled = true;
                    }
                    ctx.SaveChanges();
                }

                var vm = new PricePlanViewModel(newPlanId, user.Email, user.IsBetaUser);
                return RedirectToAction("PricePlan");
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult AddPhoneNumber()
        {
            var number = Request["new-endpoint-address"];
            return AddEndpoint(number, "Phone number", "PhoneNumbers", EndpointType.Phone);
        }

        [HttpPost]
        [Authorize]
        public ActionResult AddEmailAddress()
        {
            var number = Request["new-endpoint-address"];
            return AddEndpoint(number, "Email address", "EmailAddresses", EndpointType.Email);
        }

        [HttpPost]
        [Authorize]
        public ActionResult DeleteReminder(int id)
        {
            using (var context = new RemindersContext())
            {
                var currentUser = context.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
                var reminder = context.Reminders.SingleOrDefault(r => r.Id == id);
                if (reminder == null)
                    throw new Exception("Unknown reminder id " + id);

                var endpoint = context.Endpoints.FirstOrDefault(r => r.Address == reminder.Email && r.UserId == currentUser.UserId);
                if (endpoint == null)
                    throw new Exception("Unknown reminder id " + id);

                if(currentUser.GoogleAccount != null)
                {
                    var gs = new GoogleService(currentUser.GoogleAccount);
                    gs.DeleteReminder(reminder);
                }
                
                context.Reminders.Remove(reminder);
                context.SaveChanges();
            
                return RedirectToAction("Reminders");
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DeleteEndpoint(int id)
        {
            using (var context = new RemindersContext())
            {
                var currentUser = GetUser(context);

                var endpoint = context.Endpoints.SingleOrDefault(s => s.Id == id && s.UserId == currentUser.UserId);
                if (endpoint == null)
                    throw new Exception("Unknown endpoint id " + id);

                context.Endpoints.Remove(endpoint);
                context.SaveChanges();
                string action = "EmailAddresses";
                if (endpoint.EndpointType == EndpointType.Phone)
                    action = "PhoneNumbers";

                return RedirectToAction(action);
            }
        }

        private ActionResult AddEndpoint(string address, string fieldName, string redirectAction, EndpointType endpointType)
        {
            // todo - server side validation on endpoint counts
            using (var context = new RemindersContext())
            {
                if (String.IsNullOrEmpty(address))
                {
                    ModelState.AddModelError("new-endpoint-address", fieldName + " is required");
                    return RedirectToAction(redirectAction);
                }

                var endpoint = new Endpoint();
                endpoint.Enabled = true;
                endpoint.CreatedDateUtc = DateTime.UtcNow;
                endpoint.EndpointType = endpointType;
                endpoint.Address = address;
                endpoint.StrippedAddress = Endpoint.GenerateStrippedAddressFrom(endpoint.Address, endpointType);

                var existing = context.Endpoints.FirstOrDefault(e => e.StrippedAddress == endpoint.StrippedAddress);
                if (existing != null)
                {
                    ModelState.AddModelError("new-endpoint-address", "Sorry, that " + fieldName + " has already been registered");
                    return RedirectToAction(redirectAction); 
                }

                var currentUser = GetUser(context);
                endpoint.UserId = currentUser.UserId;
                context.Endpoints.Add(endpoint);
                context.SaveChanges();

                return RedirectToAction(redirectAction);
            }
        }

        [HttpPost]
        public JsonResult UserNameExists(string UserName)
        {
            var user = Membership.GetUser(UserName);

            return Json(user == null);
        }

        [HttpGet]
        public ActionResult GoogleCalendar()
        {
            using (var context = new RemindersContext())
            {
                var user = context.Users.Single(u => u.Username == User.Identity.Name);
                var vm = new GoogleCalendarViewModel(user.GoogleAccount, user.GoogleAccount != null ? user.GoogleAccount.Calendars.ToList() : new List<GoogleCalendar>());
                return View(vm);
            }
        }

        [HttpGet]
        public ActionResult ReminderSettings()
        {
            using (var context = new RemindersContext())
            {
                var user = GetUser(context);
                var vm = new ReminderSettingsViewModel(user);
                return View(vm);
            }
        }

        [HttpPost]
        public ActionResult ReminderSettings(ReminderSettingsViewModel vm)
        {
            using (var context = new RemindersContext())
            {
                var user = context.Users.Single(u => u.Username == User.Identity.Name);
                vm.CopySettingsToUser(user);
                context.SaveChanges();
                var c = new MemoryCacheBase();
                c.Remove(User.Identity.Name);

                vm.IsSave = true;
                return View(vm);
            }
        }

        [HttpPost]
        public HttpStatusCodeResult UpdateCalendar(int calendarId, bool isChecked)
        {
            using (var context = new RemindersContext())
            {
                var user = context.Users.Single(u => u.Username == User.Identity.Name);
                var calendar = user.GoogleAccount.Calendars.SingleOrDefault(c => c.Id == calendarId);
                calendar.SendEventsToMe = isChecked;
                context.SaveChanges();
            }
                
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
    }
}
