using IT.WebServices.Helpers;
using Microsoft.Extensions.Options;

namespace IT.WebServices.Authorization.Payment.Fortis.Clients
{
    public class FortisClient
    {
        private readonly SettingsHelper settingsHelper;

        private const string DeveloperId = "IphR7xVH";

        public readonly FortisAPI.Standard.FortisAPIClient Client;

        public FortisClient(SettingsHelper settingsHelper)
        {
            this.settingsHelper = settingsHelper;

            Client = GetClient();
        }

        private FortisAPI.Standard.FortisAPIClient GetClient()
        {
            FortisAPI.Standard.FortisAPIClient client = new FortisAPI.Standard.FortisAPIClient.Builder()
                .CustomHeaderAuthenticationCredentials(settingsHelper.Owner.Subscription.Fortis.UserID, settingsHelper.Owner.Subscription.Fortis.UserApiKey, DeveloperId)
                .Environment(settingsHelper.Public.Subscription.Fortis.IsTest ? FortisAPI.Standard.Environment.Sandbox : FortisAPI.Standard.Environment.Production)
                .HttpClientConfig(config => config.NumberOfRetries(0))
                .Build();

            return client;
        }
    }
}
