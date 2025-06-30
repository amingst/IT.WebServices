using FortisAPI.Standard.Models;
using IT.WebServices.Authorization.Payment.Fortis.Clients;
using IT.WebServices.Helpers;

namespace IT.WebServices.Authorization.Payment.Fortis.Helpers
{
    public class FortisTokenHelper
    {
        private readonly FortisClient client;
        private readonly SettingsHelper settingsHelper;

        public FortisTokenHelper(FortisClient client, SettingsHelper settingsHelper)
        {
            this.client = client;
            this.settingsHelper = settingsHelper;
        }

        public async Task<string?> GetExistingTransactionToken(string dbSubId)
        {
            try
            {
                var res = await client.Client.TokensController.ListAllTokensRelatedAsync(
                    new Page()
                    {
                        Number = 1,
                        Size = 5000,
                    },
                    null,
                    new Filter10()
                    {
                        AccountVaultApiId = dbSubId,
                    }
                );

                if (res?.List == null || res.List.Count == 0)
                    return null;

                return res.List[0].Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
            }

            return null;
        }

        public async Task<string?> GetNewPreviousTransactionToken(string tranId, string dbSubId, ResponseContact contact)
        {
            try
            {
                var token = await GetExistingTransactionToken(dbSubId);
                if (token != null)
                    return token;

                try
                {
                    var res = await client.Client.TokensController.CreateANewPreviousTransactionTokenAsync(new V1TokensPreviousTransactionRequest()
                    {
                        LocationId = settingsHelper.Owner.Subscription.Fortis.LocationID,
                        PreviousTransactionId = tranId,
                        ContactId = contact.Data.Id,
                        AccountVaultApiId = dbSubId,
                    });

                    if (res?.Data?.Id != null)
                        return res.Data.Id;
                }
                catch { }

                token = await GetExistingTransactionToken(dbSubId);
                if (token != null)
                    return token;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
            }

            return null;
        }
    }
}
