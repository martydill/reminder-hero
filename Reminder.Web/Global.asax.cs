using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Ninject;
using ReminderHero.Models;
using ReminderHero.Web.Mailers;
using ReminderHero.Web.ViewModel;
using StackExchange.Profiling;
using Stripe;

namespace ReminderHero.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            StripeConfiguration.SetApiKey(AppSettingsViewModel.StripePrivateKey);

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());

            MiniProfilerEF.InitializeEF42();

            AreaRegistration.RegisterAllAreas();
            RegisterServiceLocator();
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();

            MiniProfilerEF.Initialize();

            ConfigureAutomapper();
        }
       
        public static void ConfigureAutomapper()
        {
            AutoMapper.Mapper.CreateMap<PricePlan, PlanToSwitchTo>();

            AutoMapper.Mapper.CreateMap<Payment, PaymentViewModel>().
                ForMember(d => d.Date, o => o.MapFrom(p => p.DateUtc.ToLocalTime()));

            AutoMapper.Mapper.AssertConfigurationIsValid();
        } 

        public static void RegisterServiceLocator()
        {
            var kernel = new StandardKernel();

            RegisterServices(kernel);
            DependencyResolver.SetResolver(new Ninject.Web.Mvc.NinjectDependencyResolver(kernel));
        }

        public static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<IUserMailer>().To<UserMailer>();
        }

        protected void Application_BeginRequest()
        {
            if (Request.IsLocal)
            {
                MiniProfiler.Start();
            }
        }

        protected void Application_EndRequest()
        {
            MiniProfiler.Stop();
        }
    }
}