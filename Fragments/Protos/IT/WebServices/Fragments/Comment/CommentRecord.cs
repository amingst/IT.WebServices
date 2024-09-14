using IT.WebServices.Fragments.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pb = global::Google.Protobuf;

namespace IT.WebServices.Fragments.Comment
{
    public sealed partial class CommentRecord : pb::IMessage<CommentRecord>
    {
        public CommentResponseRecord ToCommentResponseRecord(Guid userId)
        {
            var idStr = userId.ToString();

            return new()
            {
                ContentID = Public.ContentID,
                CommentID = Public.CommentID,
                UserID = Public.UserID,
                CreatedOnUTC = Public.CreatedOnUTC,
                ModifiedOnUTC = Public.ModifiedOnUTC,
                PinnedOnUTC = Public.PinnedOnUTC,
                DeletedOnUTC = Public.DeletedOnUTC,
                CommentText = Public.Data.CommentText,
                Likes = Public.Data.Likes,
                LikedByUser = Private.Data.LikedByUserIDs.Contains(idStr),
            };
        }
    }
}
