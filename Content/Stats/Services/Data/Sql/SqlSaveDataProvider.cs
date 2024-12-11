using IT.WebServices.Fragments.Content.Stats;
using IT.WebServices.Helpers;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IT.WebServices.Content.Stats.Services.Data.Sql
{
    internal class SqlSaveDataProvider : ISaveDataProvider
    {
        private readonly MySQLHelper sql;

        public SqlSaveDataProvider(MySQLHelper sql)
        {
            this.sql = sql;
        }

        public async IAsyncEnumerable<Guid> GetAllForContent(Guid contentId)
        {
            if (contentId == Guid.Empty)
                yield break;

            const string query = @"
                SELECT
                    UserID
                FROM
                    Stats_ContentUser
                WHERE
                    ContentID = @ContentID
                    AND SavedOnUTC IS NOT NULL
                ORDER BY
                    SavedOnUTC DESC
            ";

            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("ContentID", contentId.ToString()),
            };

            using var rdr = await sql.ReturnReader(query, parameters);
            while (await rdr.ReadAsync())
            {
                var str = rdr.GetString(0);
                if (Guid.TryParse(str, out var userId))
                    yield return userId;
            }
        }

        public async IAsyncEnumerable<Guid> GetAllForUser(Guid userId)
        {
            if (userId == Guid.Empty)
                yield break;

            const string query = @"
                SELECT
                    ContentID
                FROM
                    Stats_ContentUser
                WHERE
                    UserID = @UserID
                    AND SavedOnUTC IS NOT NULL
                ORDER BY
                    SavedOnUTC DESC
            ";

            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("UserID", userId.ToString()),
            };

            using var rdr = await sql.ReturnReader(query, parameters);
            while (await rdr.ReadAsync())
            {
                var str = rdr.GetString(0);
                if (Guid.TryParse(str, out var contentId))
                    yield return contentId;
            }
        }

        public async Task Save(Guid userId, Guid contentId)
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
                    VALUES (@ContentID, @UserID,  NULL,        UTC_TIMESTAMP(),  NULL,             0,               0,              NULL,      NULL)
                    ON DUPLICATE KEY UPDATE
                            SavedOnUTC = UTC_TIMESTAMP()
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
        }

        public async Task Unsave(Guid userId, Guid contentId)
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
                    VALUES (@ContentID, @UserID,  NULL,        NULL,             NULL,             0,               0,              NULL,      NULL)
                    ON DUPLICATE KEY UPDATE
                            SavedOnUTC = NULL
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
        }
    }
}
