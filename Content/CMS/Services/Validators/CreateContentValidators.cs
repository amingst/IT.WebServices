using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IT.WebServices.Fragments.Content
{
    internal static class CreateContentValidators
    {
        public static void Validate(CreateContentRequest req, CreateContentResponse res)
        {
            ValidateContentPublicData(req.Public, res);
        }

        private static void ValidateContentPublicData(
            ContentPublicData d,
            CreateContentResponse res
        )
        {
            if (string.IsNullOrWhiteSpace(d.Title))
                res.AddError("Title", "Title is required");
            else
            {
                var title = d.Title.Trim();
                if (title.Length < 3)
                    res.AddError("Title", "Title must be at least 3 characters");
                if (title.Length > 200)
                    res.AddError("Title", "Title must not exceed 200 characters");
            }

            if (!string.IsNullOrEmpty(d.Description) && d.Description.Length > 500)
                res.AddError("Description", "Description must not exceed 500 characters");

            if (string.IsNullOrWhiteSpace(d.Author))
                res.AddError("Author", "Author is required");
            else if (d.Author.Length > 100)
                res.AddError("Author", "Author must not exceed 100 characters");

            if (string.IsNullOrWhiteSpace(d.AuthorID))
                res.AddError("AuthorID", "AuthorID is required");
            else if (!Guid.TryParse(d.AuthorID, out _))
                res.AddError("AuthorID", "AuthorID must be a valid GUID");

            if (string.IsNullOrWhiteSpace(d.URL))
                res.AddError("URL", "URL is required");
            else if (!IsRelativeUrl(d.URL))
                res.AddError("URL", "URL must be a realative URL");

            if (
                !string.IsNullOrWhiteSpace(d.FeaturedImageAssetID)
                && !Guid.TryParse(d.FeaturedImageAssetID, out _)
            )
                res.AddError("FeaturedImageAssetID", "FeaturedImageAssetID must be a valid GUID");

            if (d.SubscriptionLevel > 9999)
                res.AddError("SubscriptionLevel", "Max subscription level allowed is 9999");

            ValidateGuidList(d.CategoryIds, "CategoryIds", res);
            ValidateGuidList(d.ChannelIds, "ChannelIds", res);

            ValidateTags(d.Tags, res);

            ValidateContentVariant(d, res);
        }

        private static void ValidateContentVariant(ContentPublicData d, CreateContentResponse res)
        {
            var hasAudio = d.Audio != null;
            var hasPicture = d.Picture != null;
            var hasVideo = d.Video != null;
            var hasWritten = d.Written != null;

            var setCount =
                (hasAudio ? 1 : 0)
                + (hasPicture ? 1 : 0)
                + (hasVideo ? 1 : 0)
                + (hasWritten ? 1 : 0);
            if (setCount == 0)
            {
                res.AddError(
                    "ContentData",
                    "One content type (Audio, Picture, Video, or Written) is required"
                );
                return;
            }
            if (setCount > 1)
            {
                res.AddError("ContentData", "Exactly one content type must be provided");
                return;
            }

            if (hasAudio)
                ValidateAudio(d.Audio, res);
            if (hasPicture)
                ValidatePicture(d.Picture, res);
            if (hasVideo)
                ValidateVideo(d.Video, res);
            if (hasWritten)
                ValidateWritten(d.Written, res);
        }

        private static void ValidateAudio(AudioContentPublicData a, CreateContentResponse res)
        {
            if (!string.IsNullOrEmpty(a.HtmlBody) && a.HtmlBody.Length > 100_000)
                res.AddError("Audio.HtmlBody", "HtmlBody is too large");

            if (string.IsNullOrWhiteSpace(a.AudioAssetID))
                res.AddError("Audio.AudioAssetID", "AudioAssetID is required");
            else if (!Guid.TryParse(a.AudioAssetID, out _))
                res.AddError("Audio.AudioAssetID", "AudioAssetID must be a valid GUID");
        }

        private static void ValidatePicture(PictureContentPublicData p, CreateContentResponse res)
        {
            if (!string.IsNullOrEmpty(p.HtmlBody) && p.HtmlBody.Length > 100_000)
                res.AddError("Picture.HtmlBody", "HtmlBody is too large");

            if (p.ImageAssetIDs != null && p.ImageAssetIDs.Count > 0)
                ValidateGuidList(
                    p.ImageAssetIDs,
                    "Picture.ImageAssetIDs",
                    res,
                    requireAtLeastOne: true
                );
        }

        private static void ValidateVideo(VideoContentPublicData v, CreateContentResponse res)
        {
            if (!string.IsNullOrEmpty(v.HtmlBody) && v.HtmlBody.Length > 100_000)
                res.AddError("Video.HtmlBody", "HtmlBody is too large");

            if (!v.IsLiveStream)
            {
                var hasAnyId =
                    !string.IsNullOrWhiteSpace(v.RumbleVideoId)
                    || !string.IsNullOrWhiteSpace(v.YoutubeVideoId);
                if (!hasAnyId)
                    res.AddError(
                        "Video",
                        "At least one video id (Rumble or YouTube) is required for non-livestream content"
                    );
            }

            if (!string.IsNullOrWhiteSpace(v.RumbleVideoId) && v.RumbleVideoId.Length > 128)
                res.AddError("Video.RumbleVideoId", "RumbleVideoId is too long");
            if (!string.IsNullOrWhiteSpace(v.YoutubeVideoId) && v.YoutubeVideoId.Length > 128)
                res.AddError("Video.YoutubeVideoId", "YoutubeVideoId is too long");
        }

        private static void ValidateWritten(WrittenContentPublicData w, CreateContentResponse res)
        {
            if (string.IsNullOrWhiteSpace(w.HtmlBody))
                res.AddError("Written.HtmlBody", "HtmlBody is required");
            else if (w.HtmlBody.Length > 500_000)
                res.AddError("Written.HtmlBody", "HtmlBody is too large");
        }

        private static bool IsRelativeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            if (!url.StartsWith("/"))
                return false;

            if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
                return !uri.IsAbsoluteUri;

            return false;
        }


        private static void ValidateGuidList(
            System.Collections.Generic.IEnumerable<string> list,
            string field,
            CreateContentResponse res,
            bool requireAtLeastOne = false
        )
        {
            if (list == null)
                return;

            var arr = list.ToArray();
            if (requireAtLeastOne && arr.Length == 0)
            {
                res.AddError(field, "At least one item is required");
                return;
            }

            // dedupe + per-item checks
            foreach (var (value, index) in arr.Select((v, i) => (v, i)))
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    res.AddError($"{field}[{index}]", "Value is required");
                    continue;
                }
                if (!Guid.TryParse(value, out _))
                    res.AddError($"{field}[{index}]", "Value must be a valid GUID");
            }
        }

        private static void ValidateTags(
            System.Collections.Generic.IEnumerable<string> tags,
            CreateContentResponse res
        )
        {
            if (tags == null)
                return;

            var arr = tags.ToArray();
            if (arr.Length == 0)
                return;

            if (arr.Length > 25)
                res.AddError("Tags", "No more than 25 tags allowed");

            var tagPattern = new Regex(@"^[\p{L}\p{N}\s\-_]+$", RegexOptions.Compiled);

            foreach (var (tag, index) in arr.Select((t, i) => (t?.Trim() ?? "", i)))
            {
                if (string.IsNullOrEmpty(tag))
                {
                    res.AddError($"Tags[{index}]", "Tag cannot be empty");
                    continue;
                }
                if (tag.Length > 32)
                    res.AddError($"Tags[{index}]", "Tag must not exceed 32 characters");
                if (!tagPattern.IsMatch(tag))
                    res.AddError($"Tags[{index}]", "Tag contains invalid characters");
            }
        }
    }
}
