using System;
using System.Web.Mvc;

namespace ReminderHero.Web
{
    public sealed class NonLocalRequireHttpsAttribute : RequireHttpsAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
#if DEBUG
            return;
#else

            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            if (filterContext.HttpContext != null && filterContext.HttpContext.Request.IsLocal)
            {
                return;
            }

            base.OnAuthorization(filterContext);
#endif
        }
    }
}