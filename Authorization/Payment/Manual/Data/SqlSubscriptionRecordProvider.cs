using IT.WebServices.Authorization.Payment.Manual.Helpers;
using IT.WebServices.Fragments.Authentication;
using IT.WebServices.Fragments.Authorization.Payment.Manual;
using IT.WebServices.Fragments.Content;
using IT.WebServices.Fragments.Generic;
using IT.WebServices.Helpers;
using Microsoft.AspNetCore.Components;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IT.WebServices.Authorization.Payment.Manual.Data
{
    internal class SqlSubscriptionRecordProvider : ISubscriptionRecordProvider
    {
        public readonly MySQLHelper sql;

        public SqlSubscriptionRecordProvider(MySQLHelper sql)
        {
            this.sql = sql;
        }

        public async Task Delete(Guid userId, Guid subId)
        {
            try
            {
                const string query = @"
                    DELETE FROM
                        Payment_Manual_Subscription
                    WHERE
                        UserID = @UserID
                        AND ManualSubscriptionID = @ManualSubscriptionID;
                ";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("UserID", userId.ToString()),
                    new MySqlParameter("ManualSubscriptionID", subId.ToString()),
                };

                await sql.RunCmd(query, parameters);
            }
            catch (Exception)
            {
            }
        }

        public async Task<bool> Exists(Guid userId, Guid subId)
        {
            var rec = await GetBySubscriptionId(userId, subId);
            return rec != null;
        }

        public async IAsyncEnumerable<ManualSubscriptionRecord> GetAll()
        {
            const string query = @"
                    SELECT
                        *
                    FROM
                        Payment_Manual_Subscription
                ";

            using var rdr = await sql.ReturnReader(query);

            while (await rdr.ReadAsync())
            {
                var record = rdr.ParseManualSubscriptionRecord();

                if (record != null)
                    yield return record;
            }
        }

        public async IAsyncEnumerable<ManualSubscriptionRecord> GetAllByUserId(Guid userId)
        {
            const string query = @"
                    SELECT
                        *
                    FROM
                        Payment_Manual_Subscription
                    WHERE
                        UserID = @UserID;
                ";

            var parameters = new MySqlParameter[]
            {
                    new MySqlParameter("UserID", userId.ToString())
            };

            using var rdr = await sql.ReturnReader(query, parameters);

            while (await rdr.ReadAsync())
            {
                var record = rdr.ParseManualSubscriptionRecord();

                if (record != null)
                    yield return record;
            }
        }

        public async IAsyncEnumerable<(Guid userId, Guid subId)> GetAllSubscriptionIds()
        {
            const string query = @"
                    SELECT
                        UserID,
                        ManualSubscriptionID
                    FROM
                        Payment_Manual_Subscription
                ";

            using var rdr = await sql.ReturnReader(query);

            while (await rdr.ReadAsync())
            {
                var userId = (rdr["UserID"] as string ?? "").ToGuid();
                var subId = (rdr["ManualSubscriptionID"] as string ?? "").ToGuid();

                if (userId == Guid.Empty) continue;
                if (subId == Guid.Empty) continue;

                yield return (userId, subId);
            }
        }

        public async Task<ManualSubscriptionRecord?> GetBySubscriptionId(Guid userId, Guid subId)
        {
            try
            {
                const string query = @"
                    SELECT
                        *
                    FROM
                        Payment_Manual_Subscription
                    WHERE
                        UserID = @UserID
                        AND ManualSubscriptionID = @ManualSubscriptionID;
                ";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("UserID", userId.ToString()),
                    new MySqlParameter("ManualSubscriptionID", subId.ToString()),
                };

                using var rdr = await sql.ReturnReader(query, parameters);

                if (await rdr.ReadAsync())
                {
                    var record = rdr.ParseManualSubscriptionRecord();

                    return record;
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public Task Save(ManualSubscriptionRecord record)
        {
            return InsertOrUpdate(record);
        }

        private async Task InsertOrUpdate(ManualSubscriptionRecord record)
        {
            try
            {
                const string query = @"
                    INSERT INTO Payment_Manual_Subscription
                            (ManualSubscriptionID,  UserID,  AmountCents,
                             CreatedOnUTC,  CreatedBy,  ModifiedOnUTC,  ModifiedBy,  CanceledOnUTC,  CanceledBy)
                    VALUES (@ManualSubscriptionID, @UserID, @AmountCents,
                            @CreatedOnUTC, @CreatedBy, @ModifiedOnUTC, @ModifiedBy, @CanceledOnUTC, @CanceledBy)
                    ON DUPLICATE KEY UPDATE
                            UserID = @UserID,
                            AmountCents = @AmountCents,
                            ModifiedOnUTC = @ModifiedOnUTC,
                            ModifiedBy = @ModifiedBy,
                            CanceledOnUTC = @CanceledOnUTC,
                            CanceledBy = @CanceledBy
                ";

                var parameters = new List<MySqlParameter>()
                {
                    new MySqlParameter("ManualSubscriptionID", record.SubscriptionID),
                    new MySqlParameter("UserID", record.UserID),
                    new MySqlParameter("AmountCents", record.AmountCents),
                    new MySqlParameter("CreatedOnUTC", record.CreatedOnUTC.ToDateTime()),
                    new MySqlParameter("CreatedBy", record.CreatedBy),
                    new MySqlParameter("ModifiedOnUTC", record.ModifiedOnUTC?.ToDateTime()),
                    new MySqlParameter("ModifiedBy", record.ModifiedBy.Length == 36 ? record.ModifiedBy : null),
                    new MySqlParameter("CanceledOnUTC", record.CanceledOnUTC?.ToDateTime()),
                    new MySqlParameter("CanceledBy", record.CanceledBy.Length == 36 ? record.CanceledBy : null)
                };

                await sql.RunCmd(query, parameters.ToArray());
            }
            catch (Exception)
            {
            }
        }
    }
}
