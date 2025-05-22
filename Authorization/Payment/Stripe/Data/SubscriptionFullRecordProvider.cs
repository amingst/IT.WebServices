using Google.Protobuf;
using Microsoft.Extensions.Options;
using IT.WebServices.Fragments.Authorization.Payment.Stripe;
using IT.WebServices.Fragments.Generic;
using IT.WebServices.Models;
using Microsoft.AspNetCore.SignalR;
using IT.WebServices.Helpers;

namespace IT.WebServices.Authorization.Payment.Stripe.Data
{
    public class SubscriptionFullRecordProvider : ISubscriptionFullRecordProvider
    {
        private readonly IPaymentRecordProvider paymentProvider;
        private readonly ISubscriptionRecordProvider subProvider;

        public SubscriptionFullRecordProvider(IPaymentRecordProvider paymentProvider, ISubscriptionRecordProvider subProvider)
        {
            this.paymentProvider = paymentProvider;
            this.subProvider = subProvider;
        }

        public Task Delete(Guid userId, Guid subId)
        {
            return Task.WhenAll(
                    subProvider.Delete(userId, subId),
                    paymentProvider.DeleteAll(userId, subId)
                );
        }

        public async IAsyncEnumerable<StripeSubscriptionFullRecord> GetAll()
        {
            await foreach (var sub in subProvider.GetAll())
            {
                var full = new StripeSubscriptionFullRecord()
                {
                    SubscriptionRecord = sub
                };

                await Hydrate(full);

                yield return full;
            }
        }

        public async IAsyncEnumerable<StripeSubscriptionFullRecord> GetAllByUserId(Guid userId)
        {
            await foreach (var sub in subProvider.GetAllByUserId(userId))
            {
                var full = new StripeSubscriptionFullRecord()
                {
                    SubscriptionRecord = sub
                };

                await Hydrate(full);

                yield return full;
            }
        }

        public async Task<StripeSubscriptionFullRecord?> GetBySubscriptionId(Guid userId, Guid subId)
        {
            var sub = await subProvider.GetById(userId, subId);
            if (sub == null)
                return null;

            var full = new StripeSubscriptionFullRecord()
            {
                SubscriptionRecord = sub
            };

            await Hydrate(full);

            return full;
        }

        public async Task Save(StripeSubscriptionFullRecord full)
        {
            if (full.SubscriptionRecord == null)
                return;

            var tasks = new List<Task> { subProvider.Save(full.SubscriptionRecord) };

            foreach (var p in full.Payments)
                tasks.Add(paymentProvider.Save(p));

            await Task.WhenAll(tasks);
        }

        private async Task Hydrate(StripeSubscriptionFullRecord full)
        {
            var sub = full.SubscriptionRecord;

            full.Payments.AddRange(await paymentProvider.GetAllBySubscriptionId(sub.UserID.ToGuid(), sub.SubscriptionID.ToGuid()).ToList());

            full.CalculateRecords();
        }
    }
}
