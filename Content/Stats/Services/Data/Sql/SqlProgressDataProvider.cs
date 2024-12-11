using Google.Protobuf.WellKnownTypes;
using IT.WebServices.Fragments.Content.Stats;
using IT.WebServices.Helpers;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace IT.WebServices.Content.Stats.Services.Data.Sql
{
    internal class SqlProgressDataProvider : IProgressDataProvider
    {
        private readonly MySQLHelper sql;

        public SqlProgressDataProvider(MySQLHelper sql)
        {
            this.sql = sql;
        }

        public async Task<bool> Delete(Guid userId, Guid contentId)
        {
            if (contentId == Guid.Empty)
                return false;
            if (userId == Guid.Empty)
                return false;

            try
            {
                const string query = @"
                    INSERT INTO Stats_ContentUser
                            (ContentID,  UserID,  LikedOnUTC,  SavedOnUTC,       ViewedLastOnUTC,  NumberOfShares,  NumberOfViews,  Progress,  ProgressUpdatedOnUTC)
                    VALUES (@ContentID, @UserID,  NULL,        NULL,             NULL,             0,               0,              NULL,      NULL)
                    ON DUPLICATE KEY UPDATE
                            Progress = NULL,
                            ProgressUpdatedOnUTC = NULL
                ";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("ContentID", contentId.ToString()),
                    new MySqlParameter("UserID", userId.ToString()),
                };

                await sql.RunCmd(query, parameters);
            }
            catch (Exception)
            {
            }

            return true;
        }

        public Task<bool> Exists(Guid userId, Guid contentId)
        {
            throw new NotImplementedException();
        }

        public async Task<UserProgressRecord> Get(Guid userId, Guid contentId)
        {
            if (userId == Guid.Empty)
                return null;

            const string query = @"
                SELECT
                    ContentID,
                    Progress,
                    ProgressUpdatedOnUTC
                FROM
                    Stats_ContentUser
                WHERE
                    UserID = @UserID
                    AND ContentID = @ContentID
                ORDER BY
                    ProgressUpdatedOnUTC DESC
            ";

            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("UserID", userId.ToString()),
                new MySqlParameter("ContentID", contentId.ToString()),
            };

            using var rdr = await sql.ReturnReader(query, parameters);
            if (await rdr.ReadAsync())
                return ToUserProgressRecord(rdr);

            return null;
        }

        public async IAsyncEnumerable<UserProgressRecord> GetAllForUser(Guid userId)
        {
            if (userId == Guid.Empty)
                yield break;

            const string query = @"
                SELECT
                    ContentID,
                    Progress,
                    ProgressUpdatedOnUTC
                FROM
                    Stats_ContentUser
                WHERE
                    UserID = @UserID
                ORDER BY
                    ProgressUpdatedOnUTC DESC
            ";

            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("UserID", userId.ToString()),
            };

            using var rdr = await sql.ReturnReader(query, parameters);
            while (await rdr.ReadAsync())
            {
                yield return ToUserProgressRecord(rdr);
            }
        }

        public async Task LogProgress(Guid userId, Guid contentId, float progress)
        {
            if (contentId == Guid.Empty)
                return;
            if (userId == Guid.Empty)
                return;

            try
            {
                const string query = @"
                    INSERT INTO Stats_ContentUser
                            (ContentID,  UserID,  LikedOnUTC,  SavedOnUTC,       ViewedLastOnUTC,  NumberOfShares,  NumberOfViews,  Progress,  ProgressUpdatedOnUTC)
                    VALUES (@ContentID, @UserID,  NULL,        NULL,             NULL,             0,               0,             @Progress,  UTC_TIMESTAMP())
                    ON DUPLICATE KEY UPDATE
                            Progress = @Progress,
                            ProgressUpdatedOnUTC = UTC_TIMESTAMP()
                ";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("ContentID", contentId.ToString()),
                    new MySqlParameter("UserID", userId.ToString()),
                    new MySqlParameter("Progress", progress),
                };

                await sql.RunCmd(query, parameters);
            }
            catch (Exception)
            {
            }

            return;
        }

        private UserProgressRecord ToUserProgressRecord(DbDataReader rdr)
        {
            return new()
            {
                ContentID = rdr.GetString(0),
                Progress = rdr.GetFloat(1),
                UpdatedOnUTC = Timestamp.FromDateTime(rdr.GetDateTime(2)),
            };
        }
    }
}
