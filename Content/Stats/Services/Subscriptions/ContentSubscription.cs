using System.Threading.Tasks;
using System.Threading;
using Google.Protobuf;
using IT.WebServices.Content.Stats.Services.Data;
using IT.WebServices.Fragments.Content.Stats;
using IT.WebServices.Fragments.Generic;
using System;
using System.Threading.Channels;

namespace IT.WebServices.Content.Stats.Services.Subscriptions
{
    public class ContentSubscription
    {
        private readonly SubscriptionList subList;
        private readonly IStatsContentPublicDataProvider pubDb;
        private readonly IStatsContentPrivateDataProvider prvDb;
        private readonly ILikeDataProvider likeData;
        private readonly ISaveDataProvider saveData;
        private readonly IShareDataProvider shareData;
        private readonly IViewDataProvider viewData;

        private Task listener;

        public ContentSubscription(SubscriptionList subList, IStatsContentPublicDataProvider pubDb, IStatsContentPrivateDataProvider prvDb, ILikeDataProvider likeData, ISaveDataProvider saveData, IShareDataProvider shareData, IViewDataProvider viewData)
        {
            this.subList = subList;
            this.pubDb = pubDb;
            this.prvDb = prvDb;
            this.likeData = likeData;
            this.saveData = saveData;
            this.shareData = shareData;
            this.viewData = viewData;
        }

        public void Load()
        {
            listener = ListenForEvents();
        }

        public async Task ListenForEvents()
        {
            await foreach (var contentId in subList.ContentChanges.Reader.ReadAllAsync())
            {
                await RebuildContent(contentId);
            }
        }

        private async Task RebuildContent(Guid contentId)
        {
            try
            {
                var rPub = new StatsContentPublicRecord() { ContentID = contentId.ToString() };
                var rPrv = new StatsContentPrivateRecord() { ContentID = contentId.ToString() };

                await Task.WhenAll(
                        RebuildLikes(contentId, rPub, rPrv),
                        RebuildSaves(contentId, rPub, rPrv),
                        RebuildShares(contentId, rPub, rPrv),
                        RebuildViews(contentId, rPub, rPrv)
                    );

                await Task.WhenAll(
                        pubDb.Save(rPub),
                        prvDb.Save(rPrv)
                    );
            }
            catch { }
        }

        private async Task RebuildLikes(Guid contentId, StatsContentPublicRecord rPub, StatsContentPrivateRecord rPrv)
        {
            var likes = likeData.GetAllForContent(contentId);
            await foreach (var userId in likes)
                rPrv.LikedBy.Add(userId.ToString());

            rPub.Likes = (ulong)rPrv.LikedBy.Count;
        }

        private async Task RebuildSaves(Guid contentId, StatsContentPublicRecord rPub, StatsContentPrivateRecord rPrv)
        {
            var saves = saveData.GetAllForContent(contentId);
            await foreach (var userId in saves)
                rPrv.SavedBy.Add(userId.ToString());

            rPub.Saves = (ulong)rPrv.SavedBy.Count;
        }

        private async Task RebuildShares(Guid contentId, StatsContentPublicRecord rPub, StatsContentPrivateRecord rPrv)
        {
            var shares = shareData.GetAllCountsForContent(contentId);
            await foreach (var share in shares)
            {
                rPrv.SharedBy.Add(share.Id.ToString());
                rPub.Shares += (ulong)share.Count;
            }
        }

        private async Task RebuildViews(Guid contentId, StatsContentPublicRecord rPub, StatsContentPrivateRecord rPrv)
        {
            var views = viewData.GetAllCountsForContent(contentId);
            await foreach (var view in views)
            {
                rPrv.ViewedBy.Add(view.Id.ToString());
                rPub.Views += (ulong)view.Count;
            }
        }
    }
}
