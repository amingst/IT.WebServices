using FortisAPI.Standard.Models;
using IT.WebServices.Authorization.Payment.Fortis.Clients;
using IT.WebServices.Authorization.Payment.Fortis.Models;
using IT.WebServices.Helpers;

namespace IT.WebServices.Authorization.Payment.Fortis.Helpers
{
    public class FortisSubscriptionHelper
    {
        private readonly FortisClient client;
        private readonly FortisContactHelper contactHelper;
        private readonly FortisTokenHelper tokenHelper;
        private readonly FortisTransactionHelper tranHelper;
        private readonly SettingsHelper settingsHelper;

        public FortisSubscriptionHelper(FortisClient client, FortisContactHelper contactHelper, FortisTokenHelper tokenHelper, FortisTransactionHelper tranHelper, SettingsHelper settingsHelper)
        {
            this.client = client;
            this.contactHelper = contactHelper;
            this.tokenHelper = tokenHelper;
            this.tranHelper = tranHelper;
            this.settingsHelper = settingsHelper;
        }

        public async Task<ResponseRecurring?> Create(string tokenId, int amountCents, DateTime startDate)
        {
            try
            {
                return await client.Client.RecurringController.CreateANewRecurringRecordAsync(new V1RecurringsRequest()
                {
                    Active = ActiveEnum.Enum1,
                    AccountVaultId = tokenId,
                    Interval = 1,
                    IntervalType = IntervalTypeEnum.M,
                    LocationId = settingsHelper.Owner.Subscription.Fortis.LocationID,
                    StartDate = startDate.ToString("yyyy-MM-dd"),
                    TransactionAmount = amountCents,
                    PaymentMethod = PaymentMethodEnum.Cc,
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
            }

            return null;
        }

        public async Task<ResponseRecurring?> CreateFromTransaction(string tranId, long dbSubId, UserModel user, uint monthsForFirst)
        {
            try
            {
                var trans = await tranHelper.Get(tranId);
                if (trans == null)
                {
                    Console.WriteLine($"Error in CreateSubscriptionFromTransaction tranId={tranId}. GetTransaction returned null.");
                    return null;
                }

                if (user == null)
                {
                    Console.WriteLine($"Error in CreateSubscriptionFromTransaction tranId={tranId}. User is null.");
                    return null;
                }

                try
                {
                    if (trans.Data.AuthAmount != trans.Data.TransactionAmount)
                    {
                        return null;
                    }

                    var startDate = DateTimeOffset.FromUnixTimeSeconds(trans.Data.CreatedTs).UtcDateTime;
                    var recStartDate = startDate.AddMonths((int)monthsForFirst);

                    while (recStartDate.AddDays(-1) < DateTime.UtcNow)
                        recStartDate = recStartDate.AddDays(1);

                    var contact = await contactHelper.Create(user);
                    if (contact == null)
                        return null;

                    var token = await tokenHelper.GetNewPreviousTransactionToken(tranId, "s" + dbSubId.ToString(), contact);
                    if (token == null)
                    {
                        Console.WriteLine($"Error in CreateSubscriptionFromTransaction tranId={tranId}. Token is null. Failed to create a token.");
                        return null;
                    }

                    return await Create(token, trans.Data.TransactionAmount, recStartDate);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
            }

            return null;
        }

        public async Task<ResponseRecurring?> Cancel(string subscriptionId)
        {
            try
            {
                return await client.Client.RecurringController.DeleteRecurringRecordAsync(subscriptionId);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
            }

            return null;
        }

        public async Task<ResponseRecurring?> ChangeAmount(List6 sub, int newAmount)
        {
            try
            {
                return await client.Client.RecurringController.UpdateRecurringPaymentAsync(sub.Id, new V1RecurringsRequest1()
                {
                    TransactionAmount = newAmount,
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
            }

            return null;
        }

        public async Task<List6?> Get(string subscriptionId, bool includeTransactions = false, int triesLeft = 5)
        {
            try
            {
                var expand = new List<string>();
                if (includeTransactions)
                    expand.Add("transactions");

                var list = await client.Client.RecurringController.ListAllRecurringRecordAsync(
                        new Page() { Number = 1, Size = 1 },
                        null,
                        new Filter6()
                        {
                            LocationId = settingsHelper.Owner.Subscription.Fortis.LocationID,
                            ProductTransactionId = settingsHelper.Owner.Subscription.Fortis.ProductID,
                            Id = subscriptionId,
                        },
                        expand
                    );

                return list?.List?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                if (triesLeft > 0)
                    return await Get(subscriptionId, includeTransactions, triesLeft - 1);
                else
                    Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
            }

            return null;
        }

        public async Task<Dictionary<string, List6>?> GetAll(bool? active = null, int? amount = null, int triesLeft = 100)
        {
            int errors = 0;
            int page = 1;
            int size = 1000;

            var ret = new List<List6>();

            try
            {
                while (errors < 100)
                {
                    var list = await client.Client.RecurringController.ListAllRecurringRecordAsync(
                            new Page() { Number = page, Size = size },
                            null,
                            new Filter6()
                            {
                                Active = active is null ? null : (active == true ? ActiveEnum.Enum1 : ActiveEnum.Enum0),
                                TransactionAmount = amount,
                                LocationId = settingsHelper.Owner.Subscription.Fortis.LocationID,
                                ProductTransactionId = settingsHelper.Owner.Subscription.Fortis.ProductID,
                            }
                        );

                    if (list?.List == null)
                    {
                        errors++;
                        continue;
                    }

                    if (ret.Any(i => i.Id == list.List.FirstOrDefault()?.Id))
                        break;

                    ret.AddRange(list.List);

                    page++;

                    Console.WriteLine($"Loading Subscriptions: {ret.Count}");

                    if (list.List.Count < size)
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);

                if (triesLeft > 0)
                    return await GetAll(active, amount, triesLeft - 1);
                else
                    return null;
            }

            if (ret.Count % size == 0)
                throw new Exception($"{ret.Count} is divisible by {size} this normally indicates an error. Aborting!");

            return ret.ToDictionary(i => i.Id);
        }

        public async Task<Dictionary<string, List6>?> GetAllActiveAndCancelled()
        {
            var dict = await GetAll(true);
            if (dict == null)
                return null;

            var inactive = await GetAll(false);
            if (inactive == null)
                return null;

            var overlap = dict.Values.Where(r => inactive.ContainsKey(r.Id)).ToList();

            foreach (var i in inactive)
                if (!dict.ContainsKey(i.Key))
                    dict.Add(i.Key, i.Value);

            return dict;
        }

        public async Task<ResponseRecurringsCollection?> GetByContactId(string contactId)
        {
            try
            {
                return await client.Client.RecurringController.ListAllRecurringRecordAsync(
                        new Page() { Number = 1, Size = 5000 },
                        null,
                        new Filter6()
                        {
                            AccountVaultId = contactId
                        }
                    );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
            }

            return null;
        }
    }
}
