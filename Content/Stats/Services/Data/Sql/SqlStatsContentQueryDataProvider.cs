using IT.WebServices.Fragments.Content.Stats;
using IT.WebServices.Helpers;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT.WebServices.Content.Stats.Services.Data.Sql
{
    public class SqlStatsContentQueryDataProvider : IStatsContentPrivateDataProvider, IStatsContentPublicDataProvider, IStatsUserPrivateDataProvider, IStatsUserPublicDataProvider
    {
        private readonly MySQLHelper sql;

        public SqlStatsContentQueryDataProvider(MySQLHelper sql)
        {
            this.sql = sql;
        }

        public Task<bool> Delete(Guid recordId) => throw new NotImplementedException();
        public Task<bool> Exists(Guid recordId) => throw new NotImplementedException();
        public Task Save(StatsContentPrivateRecord record) => throw new NotImplementedException();
        public Task Save(StatsContentPublicRecord record) => throw new NotImplementedException();
        public Task Save(StatsUserPrivateRecord record) => throw new NotImplementedException();
        public Task Save(StatsUserPublicRecord record) => throw new NotImplementedException();

        IAsyncEnumerable<StatsContentPrivateRecord> IGenericRecordDataProvider<StatsContentPrivateRecord>.GetAll() => throw new NotImplementedException();
        IAsyncEnumerable<StatsContentPublicRecord> IGenericRecordDataProvider<StatsContentPublicRecord>.GetAll() => throw new NotImplementedException();
        IAsyncEnumerable<StatsUserPrivateRecord> IGenericRecordDataProvider<StatsUserPrivateRecord>.GetAll() => throw new NotImplementedException();
        IAsyncEnumerable<StatsUserPublicRecord> IGenericRecordDataProvider<StatsUserPublicRecord>.GetAll() => throw new NotImplementedException();

        async Task<StatsContentPrivateRecord> IGenericRecordDataProvider<StatsContentPrivateRecord>.GetById(Guid contentId)
        {
            var record = new StatsContentPrivateRecord() { ContentID = contentId.ToString() };

            if (contentId == Guid.Empty)
                return record;

            const string query = @"
                SELECT
                    UserID,
                    LikedOnUTC,
                    SavedOnUTC,
                    NumberOfShares,
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
                var userId = rdr.GetString(0);
                if (!rdr.IsDBNull(1))
                    record.LikedBy.Add(userId);
                if (!rdr.IsDBNull(2))
                    record.SavedBy.Add(userId);
                if (rdr.GetInt32(3) > 0)
                    record.SharedBy.Add(userId);
                if (rdr.GetInt32(4) > 0)
                    record.ViewedBy.Add(userId);
            }

            return record;
        }

        async Task<StatsContentPublicRecord> IGenericRecordDataProvider<StatsContentPublicRecord>.GetById(Guid contentId)
        {
            if (contentId == Guid.Empty)
                return new() { ContentID = contentId.ToString() };

            const string query = @"
                SELECT
                    SUM(CASE WHEN LikedOnUTC IS NOT NULL THEN 1 ELSE 0 END) Likes,
                    SUM(CASE WHEN SavedOnUTC IS NOT NULL THEN 1 ELSE 0 END) Saves,
                    SUM(NumberOfShares) Shares,
                    SUM(NumberOfViews) Views
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
            if (await rdr.ReadAsync())
            {
                return new()
                {
                    ContentID = contentId.ToString(),
                    Likes = (ulong)rdr.GetInt32(0),
                    Saves = (ulong)rdr.GetInt32(1),
                    Shares = (ulong)rdr.GetInt32(2),
                    Views = (ulong)rdr.GetInt32(3),
                };
            }

            return new() { ContentID = contentId.ToString() };
        }

        async Task<StatsUserPrivateRecord> IGenericRecordDataProvider<StatsUserPrivateRecord>.GetById(Guid userId)
        {
            var record = new StatsUserPrivateRecord() { UserID = userId.ToString() };

            if (userId == Guid.Empty)
                return record;

            const string query = @"
                SELECT
                    ContentID,
                    LikedOnUTC,
                    SavedOnUTC,
                    NumberOfShares,
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
                var contentId = rdr.GetString(0);
                if (!rdr.IsDBNull(1))
                    record.Likes.Add(contentId);
                if (!rdr.IsDBNull(2))
                    record.Saves.Add(contentId);
                if (rdr.GetInt32(3) > 0)
                    record.Shares.Add(contentId);
                if (rdr.GetInt64(4) > 0)
                    record.Views.Add(contentId);
            }

            return record;
        }

        async Task<StatsUserPublicRecord> IGenericRecordDataProvider<StatsUserPublicRecord>.GetById(Guid userId)
        {
            if (userId == Guid.Empty)
                return new() { UserID = userId.ToString() };

            const string query = @"
                SELECT
                    SUM(CASE WHEN LikedOnUTC IS NOT NULL THEN 1 ELSE 0 END) Likes,
                    SUM(CASE WHEN SavedOnUTC IS NOT NULL THEN 1 ELSE 0 END) Saves,
                    SUM(NumberOfShares) Shares,
                    SUM(NumberOfViews) Views
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
            if (await rdr.ReadAsync())
            {
                return new()
                {
                    UserID = userId.ToString(),
                    Likes = (ulong)rdr.GetInt32(0),
                    Saves = (ulong)rdr.GetInt32(1),
                    Shares = (ulong)rdr.GetInt32(2),
                    Views = (ulong)rdr.GetInt32(3),
                };
            }

            return new() { UserID = userId.ToString() };
        }
    }
}
