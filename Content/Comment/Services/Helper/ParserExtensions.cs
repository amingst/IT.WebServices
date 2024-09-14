using Google.Protobuf;
using IT.WebServices.Fragments.Authentication;
using IT.WebServices.Fragments.Comment;
using IT.WebServices.Fragments.Generic;
using System;
using System.Data.Common;

namespace IT.WebServices.Content.Comment.Services.Helper
{
    public static class ParserExtensions
    {
        public static CommentRecord ParseCommentRecord(this DbDataReader rdr)
        {
            var record = new CommentRecord()
            {
                Public = new()
                {
                    CommentID = rdr["CommentID"] as string,
                    ParentCommentID = rdr["ParentCommentID"] as string,
                    ContentID = rdr["ContentID"] as string,
                    UserID = rdr["UserID"] as string,
                    Data = new()
                    {
                        CommentText = rdr["CommentText"] as string ?? "",
                    },
                },
                Private = new()
                {
                    CreatedBy = rdr["CreatedBy"] as string ?? "",
                    ModifiedBy = rdr["ModifiedBy"] as string ?? "",
                    PinnedBy = rdr["PinnedBy"] as string ?? "",
                    DeletedBy = rdr["DeletedBy"] as string ?? "",
                    Data = new()
                    {
                    },
                },
            };

            DateTime d;
            if (!(rdr["CreatedOnUTC"] is DBNull))
            {
                d = DateTime.SpecifyKind((DateTime)rdr["CreatedOnUTC"], DateTimeKind.Utc);
                record.Public.CreatedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(d);
            }

            if (!(rdr["ModifiedOnUTC"] is DBNull))
            {
                d = DateTime.SpecifyKind((DateTime)rdr["ModifiedOnUTC"], DateTimeKind.Utc);
                record.Public.ModifiedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(d);
            }

            if (!(rdr["PinnedOnUTC"] is DBNull))
            {
                d = DateTime.SpecifyKind((DateTime)rdr["PinnedOnUTC"], DateTimeKind.Utc);
                record.Public.PinnedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(d);
            }

            if (!(rdr["DeletedOnUTC"] is DBNull))
            {
                d = DateTime.SpecifyKind((DateTime)rdr["DeletedOnUTC"], DateTimeKind.Utc);
                record.Public.DeletedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(d);
            }

            return record;
        }

        public static CommentResponseRecord ParseCommentResponseRecord(this DbDataReader rdr)
        {
            var record = new CommentResponseRecord()
            {
                ContentID = rdr["ContentID"] as string,
                CommentID = rdr["CommentID"] as string,
                UserID = rdr["UserID"] as string,
                UserName = rdr["UserName"] as string,
                UserDisplayName = rdr["UserDisplayName"] as string,
                CommentText = rdr["CommentText"] as string ?? "",
                Likes = (uint)(int)rdr["Likes"],
                LikedByUser = (int)rdr["LikedByUser"] == 1,
                NumReplies = (uint)(int)rdr["NumReplies"],
            };

            DateTime d;
            if (!(rdr["CreatedOnUTC"] is DBNull))
            {
                d = DateTime.SpecifyKind((DateTime)rdr["CreatedOnUTC"], DateTimeKind.Utc);
                record.CreatedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(d);
            }

            if (!(rdr["ModifiedOnUTC"] is DBNull))
            {
                d = DateTime.SpecifyKind((DateTime)rdr["ModifiedOnUTC"], DateTimeKind.Utc);
                record.ModifiedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(d);
            }

            if (!(rdr["PinnedOnUTC"] is DBNull))
            {
                d = DateTime.SpecifyKind((DateTime)rdr["PinnedOnUTC"], DateTimeKind.Utc);
                record.PinnedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(d);
            }

            if (!(rdr["DeletedOnUTC"] is DBNull))
            {
                d = DateTime.SpecifyKind((DateTime)rdr["DeletedOnUTC"], DateTimeKind.Utc);
                record.DeletedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(d);
            }

            return record;
        }
    }
}
