using IT.WebServices.Fragments.Content;
using System;
using System.Data.Common;

namespace IT.WebServices.Content.CMS.Services.Helpers
{
    public static class ParserExtensions
    {
        public static ContentRecord ParseContentRecord(this DbDataReader rdr)
        {
            var contentRecord = new ContentRecord()
            {
                Public = new()
                {
                    ContentID = rdr["ContentID"] as string,
                    Data = new()
                    {
                        Title = rdr["Title"] as string ?? "",
                        Description = rdr["Description"] as string ?? "",
                        Author = rdr["Author"] as string ?? "",
                        AuthorID = rdr["AuthorID"] as string ?? "",
                        URL = rdr["URL"] as string ?? "",
                        FeaturedImageAssetID = rdr["FeaturedImageAssetID"] as string ?? "",
                        SubscriptionLevel = (uint)(int)rdr["SubscriptionLevel"],
                    },
                },
                Private = new()
                {
                    CreatedBy = rdr["CreatedBy"] as string ?? "",
                    ModifiedBy = rdr["ModifiedBy"] as string ?? "",
                    PublishedBy = rdr["PublishedBy"] as string ?? "",
                    AnnouncedBy = rdr["AnnouncedBy"] as string ?? "",
                    PinnedBy = rdr["PinnedBy"] as string ?? "",
                    DeletedBy = rdr["DeletedBy"] as string ?? "",
                    Data = new()
                    {
                        OldContentID = rdr["OldContentID"] as string ?? "",
                    },
                },
            };

            var categories = rdr["categories"] as string;
            if (!string.IsNullOrWhiteSpace(categories))
                contentRecord.Public.Data.CategoryIds.AddRange(categories.Split(','));

            var channels = rdr["channels"] as string;
            if (!string.IsNullOrWhiteSpace(channels))
                contentRecord.Public.Data.ChannelIds.AddRange(channels.Split(','));

            DateTime d;
            if (!(rdr["CreatedOnUTC"] is DBNull))
            {
                d = DateTime.SpecifyKind((DateTime)rdr["CreatedOnUTC"], DateTimeKind.Utc);
                contentRecord.Public.CreatedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(d);
            }

            if (!(rdr["ModifiedOnUTC"] is DBNull))
            {
                d = DateTime.SpecifyKind((DateTime)rdr["ModifiedOnUTC"], DateTimeKind.Utc);
                contentRecord.Public.ModifiedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(d);
            }

            if (!(rdr["PublishOnUTC"] is DBNull))
            {
                d = DateTime.SpecifyKind((DateTime)rdr["PublishOnUTC"], DateTimeKind.Utc);
                contentRecord.Public.PublishOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(d);
            }

            if (!(rdr["AnnounceOnUTC"] is DBNull))
            {
                d = DateTime.SpecifyKind((DateTime)rdr["AnnounceOnUTC"], DateTimeKind.Utc);
                contentRecord.Public.AnnounceOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(d);
            }

            if (!(rdr["PinnedOnUTC"] is DBNull))
            {
                d = DateTime.SpecifyKind((DateTime)rdr["PinnedOnUTC"], DateTimeKind.Utc);
                contentRecord.Public.PinnedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(d);
            }

            if (!(rdr["DeletedOnUTC"] is DBNull))
            {
                d = DateTime.SpecifyKind((DateTime)rdr["DeletedOnUTC"], DateTimeKind.Utc);
                contentRecord.Public.DeletedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(d);
            }

            var type = rdr["Type"];
            switch (type)
            {
                case "Audio":
                    contentRecord.Public.Data.Audio = new()
                    {
                        AudioAssetID = rdr["AudioAssetID"] as string ?? "",
                        HtmlBody = rdr["HtmlBody"] as string ?? "",
                    };
                    contentRecord.Private.Data.Audio = new()
                    {
                    };
                    break;
                case "Picture":
                    contentRecord.Public.Data.Picture = new()
                    {
                        HtmlBody = rdr["HtmlBody"] as string ?? "",
                    };
                    contentRecord.Private.Data.Picture = new()
                    {
                    };
                    break;
                case "Video":
                    contentRecord.Public.Data.Video = new()
                    {
                        HtmlBody = rdr["HtmlBody"] as string ?? "",
                        RumbleVideoId = rdr["RumbleVideoId"] as string ?? "",
                        YoutubeVideoId = rdr["YoutubeVideoId"] as string ?? "",
                        IsLiveStream = (ulong)rdr["IsLiveStream"] == 1,
                        IsLive = (ulong)rdr["IsLive"] == 1,
                    };
                    contentRecord.Private.Data.Video = new()
                    {
                    };
                    break;
                case "Written":
                    contentRecord.Public.Data.Written = new()
                    {
                        HtmlBody = rdr["HtmlBody"] as string ?? "",
                    };
                    contentRecord.Private.Data.Written = new()
                    {
                    };
                    break;
            }

            return contentRecord;
        }
    }
}
