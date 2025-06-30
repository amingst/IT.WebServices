using Google.Protobuf;
using Microsoft.Extensions.Options;
using IT.WebServices.Fragments.Authorization.Payment.Fortis;
using IT.WebServices.Fragments.Generic;
using IT.WebServices.Models;
using Microsoft.AspNetCore.SignalR;
using IT.WebServices.Helpers;

namespace IT.WebServices.Authorization.Payment.Fortis.Data
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

        public async IAsyncEnumerable<FortisSubscriptionFullRecord> GetAll()
        {
            await foreach (var sub in subProvider.GetAll())
            {
                var full = new FortisSubscriptionFullRecord()
                {
                    SubscriptionRecord = sub
                };

                await Hydrate(full);

                yield return full;
            }
        }

        public async IAsyncEnumerable<FortisSubscriptionFullRecord> GetAllByUserId(Guid userId)
        {
            await foreach (var sub in subProvider.GetAllByUserId(userId))
            {
                var full = new FortisSubscriptionFullRecord()
                {
                    SubscriptionRecord = sub
                };

                await Hydrate(full);

                yield return full;
            }
        }

        public async Task<FortisSubscriptionFullRecord?> GetBySubscriptionId(Guid userId, Guid subId)
        {
            var sub = await subProvider.GetById(userId, subId);
            if (sub == null)
                return null;

            var full = new FortisSubscriptionFullRecord()
            {
                SubscriptionRecord = sub
            };

            await Hydrate(full);

            return full;
        }

        public async Task Save(FortisSubscriptionFullRecord full)
        {
            if (full.SubscriptionRecord == null)
                return;

            var tasks = new List<Task> { subProvider.Save(full.SubscriptionRecord) };

            foreach (var p in full.Payments)
                tasks.Add(paymentProvider.Save(p));

            await Task.WhenAll(tasks);
        }

        private async Task Hydrate(FortisSubscriptionFullRecord full)
        {
            var sub = full.SubscriptionRecord;

            full.Payments.AddRange(await paymentProvider.GetAllBySubscriptionId(sub.UserID.ToGuid(), sub.SubscriptionID.ToGuid()).ToList());

            full.CalculateRecords();
        }
    }
}
