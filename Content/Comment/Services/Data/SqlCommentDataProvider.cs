using IT.WebServices.Content.Comment.Services.Helper;
using IT.WebServices.Fragments.Authentication;
using IT.WebServices.Fragments.Comment;
using IT.WebServices.Fragments.Generic;
using IT.WebServices.Helpers;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading.Tasks;

namespace IT.WebServices.Content.Comment.Services.Data
{
    internal class SqlCommentDataProvider : ICommentDataProvider
    {
        public readonly MySQLHelper sql;

        public SqlCommentDataProvider(MySQLHelper sql)
        {
            this.sql = sql;
        }

        public Task CreateIndexes(CommentRecord record) => Task.CompletedTask;

        public Task<bool> Delete(CommentRecord record) => Delete(record.Public.CommentID.ToGuid());

        public async Task<bool> Delete(Guid commentId)
        {
            try
            {
                const string query = @"
                    DELETE FROM
                        Comment_Comment
                    WHERE
                        CommentID = @CommentID
                ";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("CommentID", commentId.ToString())
                };

                await sql.RunCmd(query, parameters);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Task DeleteIndexes(CommentRecord record) => Task.CompletedTask;

        public async Task<bool> Exists(Guid commentId)
        {
            try
            {
                const string query = @"
                    SELECT
                        1
                    FROM
                        Comment_Comment
                    WHERE
                        CommentID = @CommentID
                ";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("CommentID", commentId.ToString())
                };

                using var rdr = await sql.ReturnReader(query, parameters);

                if (await rdr.ReadAsync())
                    return true;

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<CommentRecord> Get(Guid commentId)
        {
            try
            {
                const string query = @"
                    SELECT
                        *
                    FROM
                        Comment_Comment
                    WHERE
                        CommentID = @CommentID;
                ";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("CommentID", commentId.ToString())
                };

                using var rdr = await sql.ReturnReader(query, parameters);

                if (await rdr.ReadAsync())
                {
                    var record = rdr.ParseCommentRecord();

                    await AddLikesToComment(record);

                    return record;
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public IAsyncEnumerable<CommentRecord> GetAll()
        {
            const string query = @"
                SELECT
                    *
                FROM
                    Comment_Comment
            ";

            return GetAllByQuery(query);
        }

        public IAsyncEnumerable<CommentRecord> GetByContentId(Guid contentId)
        {
            const string query = @"
                SELECT
                    *
                FROM
                    Comment_Comment
                WHERE
                    ContentID = @ContentID
            ";

            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("ContentID", contentId.ToString())
            };

            return GetAllByQuery(query);
        }

        public IAsyncEnumerable<CommentRecord> GetByParentId(Guid parentId)
        {
            const string query = @"
                SELECT
                    *
                FROM
                    Comment_Comment
                WHERE
                    ParentCommentID = @ParentCommentID
            ";

            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("ParentCommentID", parentId.ToString())
            };

            return GetAllByQuery(query);
        }

        public IAsyncEnumerable<CommentResponseRecord> GetCommentResponseRecordByContentId(Guid contentId, Guid userId)
        {
            const string query = @"
                SELECT
                    o.*,
                    SUM(CASE WHEN rc.CommentID IS NOT NULL THEN 1 ELSE 0 END) NumReplies
                FROM
                    (
                        SELECT
                            c.ContentID,
                            c.CommentID,
                            c.UserID,
                            u.UserName,
                            u.DisplayName as UserDisplayName,
                            c.CreatedOnUTC,
                            c.ModifiedOnUTC,
                            c.PinnedOnUTC,
                            c.DeletedOnUTC,
                            c.CommentText,
                            SUM(CASE WHEN l.CommentID IS NOT NULL THEN 1 ELSE 0 END) Likes,
                            SUM(CASE WHEN l.UserID = @UserID THEN 1 ELSE 0 END) LikedByUser
                        FROM
                            Comment_Comment c
                            LEFT JOIN Auth_User u ON c.UserID = u.UserID
                            LEFT JOIN Comment_Like l ON c.CommentID = l.CommentID
                        WHERE
                            ContentID = @ContentID
                        GROUP BY
                            c.CommentID
                    ) o
                    LEFT JOIN Comment_Comment rc ON o.CommentID = rc.ParentCommentID
                GROUP BY
                    o.CommentID
            ";

            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("ContentID", contentId.ToString()),
                new MySqlParameter("UserID", userId.ToString()),
            };

            return GetAllCommentResponseRecordByQuery(query);
        }

        public IAsyncEnumerable<CommentResponseRecord> GetCommentResponseRecordByParentId(Guid parentId, Guid userId)
        {
            const string query = @"
                SELECT
                    o.*,
                    SUM(CASE WHEN rc.CommentID IS NOT NULL THEN 1 ELSE 0 END) NumReplies
                FROM
                    (
                        SELECT
                            c.ContentID,
                            c.CommentID,
                            c.UserID,
                            u.UserName,
                            u.DisplayName as UserDisplayName,
                            c.CreatedOnUTC,
                            c.ModifiedOnUTC,
                            c.PinnedOnUTC,
                            c.DeletedOnUTC,
                            c.CommentText,
                            SUM(CASE WHEN l.CommentID IS NOT NULL THEN 1 ELSE 0 END) Likes,
                            SUM(CASE WHEN l.UserID = @UserID THEN 1 ELSE 0 END) LikedByUser
                        FROM
                            Comment_Comment c
                            LEFT JOIN Auth_User u ON c.UserID = u.UserID
                            LEFT JOIN Comment_Like l ON c.CommentID = l.CommentID
                        WHERE
                            ParentCommentID = @ParentCommentID
                        GROUP BY
                            c.CommentID
                    ) o
                    LEFT JOIN Comment_Comment rc ON o.CommentID = rc.ParentCommentID
                GROUP BY
                    o.CommentID
            ";

            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("ParentCommentID", parentId.ToString()),
                new MySqlParameter("UserID", userId.ToString()),
            };

            return GetAllCommentResponseRecordByQuery(query);
        }

        public Task Insert(CommentRecord record)
        {
            return InsertOrUpdate(record);
        }

        public Task Update(CommentRecord record)
        {
            return InsertOrUpdate(record);
        }

        private async Task AddLikesToComment(CommentRecord record)
        {
            const string query = @"
                    SELECT
                        UserID
                    FROM
                        Comment_Like
                    WHERE
                        CommentID = @CommentID
                ";

            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("CommentID", record.Public.CommentID)
            };

            using var rdr = await sql.ReturnReader(query, parameters);

            while (await rdr.ReadAsync())
                record.Private.Data.LikedByUserIDs.Add(rdr[0] as string);

            record.Public.Data.Likes = (uint)record.Private.Data.LikedByUserIDs.Count;
        }

        private async IAsyncEnumerable<CommentRecord> GetAllByQuery(string query, params MySqlParameter[] pList)
        {
            using var rdr = await sql.ReturnReader(query, pList);

            while (await rdr.ReadAsync())
            {
                var record = rdr.ParseCommentRecord();

                await AddLikesToComment(record);

                yield return record;
            }
        }

        private async IAsyncEnumerable<CommentResponseRecord> GetAllCommentResponseRecordByQuery(string query, params MySqlParameter[] pList)
        {
            using var rdr = await sql.ReturnReader(query, pList);

            while (await rdr.ReadAsync())
            {
                var record = rdr.ParseCommentResponseRecord();

                yield return record;
            }
        }

        private async Task InsertOrUpdate(CommentRecord record)
        {
            try
            {
                const string query = @"
                    INSERT INTO Comment_Comment
                            (CommentID,  ParentCommentID,  ContentID,  UserID,  CommentText, CreatedOnUTC,  CreatedBy,
                            ModifiedOnUTC,  ModifiedBy,  PinnedOnUTC,  PinnedBy,  DeletedOnUTC,  DeletedBy)
                    VALUES (@CommentID, @ParentCommentID, @ContentID, @UserID, @CommentText, @CreatedOnUTC, @CreatedBy,
                           @ModifiedOnUTC, @ModifiedBy, @PinnedOnUTC, @PinnedBy, @DeletedOnUTC, @DeletedBy)
                    ON DUPLICATE KEY UPDATE
                            CommentText = @CommentText,
                            ModifiedOnUTC = @ModifiedOnUTC,
                            ModifiedBy = @ModifiedBy,
                            PinnedOnUTC = @PinnedOnUTC,
                            PinnedBy = @PinnedBy,
                            DeletedOnUTC = @DeletedOnUTC,
                            DeletedBy = @DeletedBy
                ";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("CommentID", record.Public.CommentID),
                    new MySqlParameter("ParentCommentID", record.Public.ParentCommentID),
                    new MySqlParameter("ContentID", record.Public.ContentID),
                    new MySqlParameter("UserID", record.Public.UserID),
                    new MySqlParameter("CommentText", record.Public.Data.CommentText),
                    new MySqlParameter("CreatedOnUTC", record.Public.CreatedOnUTC.ToDateTime()),
                    new MySqlParameter("CreatedBy", record.Private.CreatedBy),
                    new MySqlParameter("ModifiedOnUTC", record.Public.ModifiedOnUTC.ToDateTime()),
                    new MySqlParameter("ModifiedBy", record.Private.ModifiedBy),
                    new MySqlParameter("PinnedOnUTC", record.Public.PinnedOnUTC.ToDateTime()),
                    new MySqlParameter("PinnedBy", record.Private.PinnedBy),
                    new MySqlParameter("DeletedOnUTC", record.Public.DeletedOnUTC.ToDateTime()),
                    new MySqlParameter("DeletedBy", record.Private.DeletedBy),
                };

                await sql.RunCmd(query, parameters);
            }
            catch (Exception)
            {
            }
        }
    }
}
