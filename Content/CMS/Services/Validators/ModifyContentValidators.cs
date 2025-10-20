using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IT.WebServices.Fragments.Content;

internal class ModifyContentValidators
{
    public static void Validate(ModifyContentRequest req, ModifyContentResponse res)
    {
        if (req?.Public == null) return;
        ValidatePublicData(req.Public, res);
    }

    private static void ValidatePublicData(ContentPublicData d, ModifyContentResponse res)
    {
        if (Provided(d.Title))
        {
            var title = d.Title.Trim();
            if (title.Length < 3) res.AddError("Title", "Title must be at least 3 characters");
            if (title.Length > 200) res.AddError("Title", "Title must not exceed 200 characters");
        }

        if (Provided(d.Description) && d.Description.Length > 500)
            res.AddError("Description", "Description must not exceed 500 characters");

        if (Provided(d.Author) && d.Author.Length > 100)
            res.AddError("Author", "Author must not exceed 100 characters");

        if (Provided(d.AuthorID) && !Guid.TryParse(d.AuthorID, out _))
            res.AddError("AuthorID", "AuthorID must be a valid GUID");

        if (Provided(d.URL) && !IsRelativeUrl(d.URL))
            res.AddError("URL", "URL must be a realative URL");

        if (Provided(d.FeaturedImageAssetID) && !Guid.TryParse(d.FeaturedImageAssetID, out _))
            res.AddError("FeaturedImageAssetID", "FeaturedImageAssetID must be a valid GUID");

        if (d.SubscriptionLevel > 0 && d.SubscriptionLevel > 9999)
            res.AddError("SubscriptionLevel", "Max subscription level allowed is 9999");

        ValidateGuidList(d.CategoryIds, "CategoryIds", res);
        ValidateGuidList(d.ChannelIds, "ChannelIds", res);

        ValidateTags(d.Tags, res);

        ValidateContentVariant(d, res);
    }

    private static void ValidateContentVariant(ContentPublicData d, ModifyContentResponse res)
    {
        var hasAudio = d.Audio != null;
        var hasPicture = d.Picture != null;
        var hasVideo = d.Video != null;
        var hasWritten = d.Written != null;

        var setCount = (hasAudio ? 1 : 0) + (hasPicture ? 1 : 0) + (hasVideo ? 1 : 0) + (hasWritten ? 1 : 0);
        if (setCount == 0) return;

        if (setCount > 1)
        {
            res.AddError("ContentData", "Exactly one content type must be provided when updating content data");
            return;
        }

        if (hasAudio) ValidateAudio(d.Audio, res);
        if (hasPicture) ValidatePicture(d.Picture, res);
        if (hasVideo) ValidateVideo(d.Video, res);
        if (hasWritten) ValidateWritten(d.Written, res);
    }

    private static void ValidateAudio(AudioContentPublicData a, ModifyContentResponse res)
    {
        if (Provided(a.HtmlBody) && a.HtmlBody.Length > 100_000)
            res.AddError("Audio.HtmlBody", "HtmlBody is too large");

        if (Provided(a.AudioAssetID) && !Guid.TryParse(a.AudioAssetID, out _))
            res.AddError("Audio.AudioAssetID", "AudioAssetID must be a valid GUID");
    }

    private static void ValidatePicture(PictureContentPublicData p, ModifyContentResponse res)
    {
        if (Provided(p.HtmlBody) && p.HtmlBody.Length > 100_000)
            res.AddError("Picture.HtmlBody", "HtmlBody is too large");

        if (p.ImageAssetIDs != null && p.ImageAssetIDs.Count > 0)
            ValidateGuidList(p.ImageAssetIDs, "Picture.ImageAssetIDs", res);
    }

    private static void ValidateVideo(VideoContentPublicData v, ModifyContentResponse res)
    {
        if (Provided(v.HtmlBody) && v.HtmlBody.Length > 100_000)
            res.AddError("Video.HtmlBody", "HtmlBody is too large");

        if (!v.IsLiveStream)
        {
            if (Provided(v.RumbleVideoId) && v.RumbleVideoId.Length > 128)
                res.AddError("Video.RumbleVideoId", "RumbleVideoId is too long");
            if (Provided(v.YoutubeVideoId) && v.YoutubeVideoId.Length > 128)
                res.AddError("Video.YoutubeVideoId", "YoutubeVideoId is too long");
        }
    }

    private static void ValidateWritten(WrittenContentPublicData w, ModifyContentResponse res)
    {
        if (Provided(w.HtmlBody) && w.HtmlBody.Length > 500_000)
            res.AddError("Written.HtmlBody", "HtmlBody is too large");
    }

    private static bool Provided(string s) => !string.IsNullOrWhiteSpace(s);

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


    private static void ValidateGuidList(System.Collections.Generic.IEnumerable<string> list, string field, ModifyContentResponse res)
    {
        if (list == null) return;
        var arr = list.ToArray();
        if (arr.Length == 0) return;

        foreach (var (value, index) in arr.Select((v, i) => (v, i)))
        {
            if (!Provided(value))
            {
                res.AddError($"{field}[{index}]", "Value is required");
                continue;
            }
            if (!Guid.TryParse(value, out _))
                res.AddError($"{field}[{index}]", "Value must be a valid GUID");
        }
    }

    private static void ValidateTags(System.Collections.Generic.IEnumerable<string> tags, ModifyContentResponse res)
    {
        if (tags == null) return;
        var arr = tags.ToArray();
        if (arr.Length == 0) return;

        if (arr.Length > 25)
            res.AddError("Tags", "No more than 25 tags allowed");

        var tagPattern = new Regex(@"^[\p{L}\p{N}\s\-_]+$", RegexOptions.Compiled);
        foreach (var (tag, index) in arr.Select((t, i) => (t?.Trim() ?? "", i)))
        {
            if (tag.Length == 0)
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
