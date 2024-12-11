using IT.WebServices.Fragments.Content.Stats;
using IT.WebServices.Helpers;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IT.WebServices.Content.Stats.Services.Data.Sql
{
    internal class SqlViewDataProvider : IViewDataProvider
    {
        private readonly MySQLHelper sql;

        public SqlViewDataProvider(MySQLHelper sql)
        {
            this.sql = sql;
        }

        public async IAsyncEnumerable<IQueryableTimestampDataProvider.Data> GetAllCountsForContent(Guid contentId)
        {
            if (contentId == Guid.Empty)
                yield break;

            const string query = @"
                SELECT
                    UserID,
                    NumberOfViews
                FROM
                    Stats_ContentUser
                WHERE
                    ContentID = @ContentID
            ";

            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("ContentID", contentId.ToString()),
            };

            using var rdr = await sql.ReturnReader(query, parameters);
            while (await rdr.ReadAsync())
            {
                var str = rdr.GetString(0);
                if (!Guid.TryParse(str, out var userId))
                    continue;

                yield return new(userId, rdr.GetInt32(1));
            }
        }

        public async IAsyncEnumerable<IQueryableTimestampDataProvider.Data> GetAllCountsForUser(Guid userId)
        {
            if (userId == Guid.Empty)
                yield break;

            const string query = @"
                SELECT
                    ContentID,
                    NumberOfViews
                FROM
                    Stats_ContentUser
                WHERE
                    UserID = @UserID
            ";

            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("UserID", userId.ToString()),
            };

            using var rdr = await sql.ReturnReader(query, parameters);
            while (await rdr.ReadAsync())
            {
                var str = rdr.GetString(0);
                if (!Guid.TryParse(str, out var contentId))
                    continue;

                yield return new(contentId, rdr.GetInt32(1));
            }
        }

        public async Task LogView(Guid userId, Guid contentId)
        {
            if (contentId == Guid.Empty)
                return;

            try
            {
                const string query = @"
                    INSERT INTO Stats_Views
                            (ContentID,  UserID,  ViewedOnUTC)
                    VALUES (@ContentID, @UserID,  UTC_TIMESTAMP())
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

            try
            {
                const string query = @"
                    INSERT INTO Stats_ContentUser
                            (ContentID,  UserID,  LikedOnUTC,  SavedOnUTC,  ViewedLastOnUTC,  NumberOfShares,  NumberOfViews,  Progress,  ProgressUpdatedOnUTC)
                    VALUES (@ContentID, @UserID,  NULL,        NULL,        UTC_TIMESTAMP(),  0,               1,              NULL,      NULL)
                    ON DUPLICATE KEY UPDATE
                            ViewedLastOnUTC = UTC_TIMESTAMP(),
                            NumberOfViews = NumberOfViews + 1
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
