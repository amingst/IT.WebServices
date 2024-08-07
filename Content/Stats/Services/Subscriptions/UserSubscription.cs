using System.Threading.Tasks;
using System.Threading;
using Google.Protobuf;
using IT.WebServices.Content.Stats.Services.Data;
using IT.WebServices.Fragments.Content.Stats;
using IT.WebServices.Fragments.Generic;
using System;
using Microsoft.Extensions.Logging;

namespace IT.WebServices.Content.Stats.Services.Subscriptions
{
    public class UserSubscription
    {
        private readonly SubscriptionList subList;
        private readonly IStatsUserPublicDataProvider pubDb;
        private readonly IStatsUserPrivateDataProvider prvDb;
        private readonly ILikeDataProvider likeData;
        private readonly ISaveDataProvider saveData;

        private Task listener;

        public UserSubscription(SubscriptionList subList, IStatsUserPublicDataProvider pubDb, IStatsUserPrivateDataProvider prvDb, ILikeDataProvider likeData, ISaveDataProvider saveData)
        {
            this.subList = subList;
            this.pubDb = pubDb;
            this.prvDb = prvDb;
            this.likeData = likeData;
            this.saveData = saveData;
        }

        public void Load()
        {
            listener = ListenForEvents();
        }

        public async Task ListenForEvents()
        {
            await foreach (var userId in subList.UserChanges.Reader.ReadAllAsync())
            {
                await RebuildUser(userId);
            }
        }

        private async Task RebuildUser(Guid userId)
        {
            var rPub = new StatsUserPublicRecord() { UserID = userId.ToString() };
            var rPrv = new StatsUserPrivateRecord() { UserID = userId.ToString() };

            await Task.WhenAll(
                    RebuildLikes(userId, rPub, rPrv),
                    RebuildSaves(userId, rPub, rPrv)
                );

            await Task.WhenAll(
                    pubDb.Save(rPub),
                    prvDb.Save(rPrv)
                );
        }

        private async Task RebuildLikes(Guid userId, StatsUserPublicRecord rPub, StatsUserPrivateRecord rPrv)
        {
            var likes = likeData.GetAllForUser(userId);
            await foreach (var contentId in likes)
                rPrv.Likes.Add(contentId.ToString());

            rPub.Likes = (ulong)rPrv.Likes.Count;
        }

        private async Task RebuildSaves(Guid userId, StatsUserPublicRecord rPub, StatsUserPrivateRecord rPrv)
        {
            var saves = saveData.GetAllForUser(userId);
            await foreach (var contentId in saves)
                rPrv.Saves.Add(contentId.ToString());

            rPub.Saves = (ulong)rPrv.Saves.Count;
        }
    }
}
