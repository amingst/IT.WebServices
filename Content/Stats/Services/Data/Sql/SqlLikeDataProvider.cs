using IT.WebServices.Content.Stats.Services.Data.Sql.Models;
using IT.WebServices.Fragments.Content.Stats;
using IT.WebServices.Helpers;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IT.WebServices.Content.Stats.Services.Data.Sql
{
    internal class SqlLikeDataProvider : ILikeDataProvider
    {
        private readonly MySQLHelper sql;

        public SqlLikeDataProvider(MySQLHelper sql)
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
                    AND LikedOnUTC IS NOT NULL
                ORDER BY
                    LikedOnUTC DESC
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
                    AND LikedOnUTC IS NOT NULL
                ORDER BY
                    LikedOnUTC DESC
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

        public async Task Like(Guid userId, Guid contentId)
        {
            if (contentId == Guid.Empty)
                return;
            if (userId == Guid.Empty)
                return;

            await Task.WhenAll(
                StatsContentUserModel.SetLiked(sql, contentId, userId),
                StatsContentModel.IncrementLikes(sql, contentId),
                StatsUserModel.IncrementLikes(sql, userId)
            );
        }

        public async Task Unlike(Guid userId, Guid contentId)
        {
            if (contentId == Guid.Empty)
                return;
            if (userId == Guid.Empty)
                return;

            await Task.WhenAll(
                StatsContentUserModel.SetUnLiked(sql, contentId, userId),
                StatsContentModel.DecrementLikes(sql, contentId),
                StatsUserModel.DecrementLikes(sql, userId)
            );
        }
    }
}
