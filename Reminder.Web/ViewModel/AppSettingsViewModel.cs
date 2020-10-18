
namespace ReminderHero.Web.ViewModel
{
    public static class AppSettingsViewModel
    {
        private static readonly string StripePublicKeyKey = "StripePublicKey";

        public static string StripePubilcKey
        {
            get
            {
                var stripePublicKey = System.Configuration.ConfigurationManager.AppSettings[StripePublicKeyKey];
                return stripePublicKey;
            }
        }

        private static readonly string StripePrivateKeyKey = "StripePrivateKey";

        public static string StripePrivateKey
        {
            get
            {
                var stripePublicKey = System.Configuration.ConfigurationManager.AppSettings[StripePrivateKeyKey];
                return stripePublicKey;
            }
        }
    }
}