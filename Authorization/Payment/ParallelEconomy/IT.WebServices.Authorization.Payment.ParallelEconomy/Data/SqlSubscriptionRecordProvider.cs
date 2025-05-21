using IT.WebServices.Authorization.Payment.ParallelEconomy.Helpers;
using IT.WebServices.Fragments.Authentication;
using IT.WebServices.Fragments.Authorization.Payment.ParallelEconomy;
using IT.WebServices.Fragments.Content;
using IT.WebServices.Fragments.Generic;
using IT.WebServices.Helpers;
using Microsoft.AspNetCore.Components;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IT.WebServices.Authorization.Payment.ParallelEconomy.Data
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
                        Payment_PE_Subscription
                    WHERE
                        UserID = @UserID
                        AND PEInternalSubscriptionID = @PEInternalSubscriptionID;
                ";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("UserID", userId.ToString()),
                    new MySqlParameter("PEInternalSubscriptionID", subId.ToString()),
                };

                await sql.RunCmd(query, parameters);
            }
            catch (Exception)
            {
            }
        }

        public async Task<bool> Exists(Guid userId, Guid subId)
        {
            var rec = await GetById(userId, subId);
            return rec != null;
        }

        public async IAsyncEnumerable<ParallelEconomySubscriptionRecord> GetAll()
        {
            const string query = @"
                    SELECT
                        *
                    FROM
                        Payment_PE_Subscription
                ";

            using var rdr = await sql.ReturnReader(query);

            while (await rdr.ReadAsync())
            {
                var record = rdr.ParseParallelEconomySubscriptionRecord();

                if (record != null)
                    yield return record;
            }
        }

        public async IAsyncEnumerable<ParallelEconomySubscriptionRecord> GetAllByUserId(Guid userId)
        {
            const string query = @"
                    SELECT
                        *
                    FROM
                        Payment_PE_Subscription
                    WHERE
                        UserID = @UserID;
                ";

            var parameters = new MySqlParameter[]
            {
                    new MySqlParameter("UserID", userId.ToString())
            };

            using var rdr = await sql.ReturnReader(query);

            while (await rdr.ReadAsync())
            {
                var record = rdr.ParseParallelEconomySubscriptionRecord();

                if (record != null)
                    yield return record;
            }
        }

        public async IAsyncEnumerable<(Guid userId, Guid subId)> GetAllSubscriptionIds()
        {
            const string query = @"
                    SELECT
                        UserID,
                        PEInternalSubscriptionID
                    FROM
                        Payment_PE_Subscription
                ";

            using var rdr = await sql.ReturnReader(query);

            while (await rdr.ReadAsync())
            {
                var userId = (rdr["UserID"] as string ?? "").ToGuid();
                var subId = (rdr["PEInternalSubscriptionID"] as string ?? "").ToGuid();

                if (userId == Guid.Empty) continue;
                if (subId == Guid.Empty) continue;

                yield return (userId, subId);
            }
        }

        public async Task<ParallelEconomySubscriptionRecord?> GetById(Guid userId, Guid subId)
        {
            try
            {
                const string query = @"
                    SELECT
                        *
                    FROM
                        Payment_PE_Subscription
                    WHERE
                        UserID = @UserID
                        AND PEInternalSubscriptionID = @PEInternalSubscriptionID;
                ";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("UserID", userId.ToString()),
                    new MySqlParameter("PEInternalSubscriptionID", subId.ToString()),
                };

                using var rdr = await sql.ReturnReader(query, parameters);

                if (await rdr.ReadAsync())
                {
                    var record = rdr.ParseParallelEconomySubscriptionRecord();

                    return record;
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public Task Save(ParallelEconomySubscriptionRecord record)
        {
            return InsertOrUpdate(record);
        }

        private async Task InsertOrUpdate(ParallelEconomySubscriptionRecord record)
        {
            try
            {
                const string query = @"
                    INSERT INTO Payment_PE_Subscription
                            (PEInternalSubscriptionID,  UserID,  PECustomerID,  PESubscriptionID,  Status,
                             AmountCents,  TaxCents,  TaxRateThousandPercents,  TotalCents,
                             CreatedOnUTC,  CreatedBy,  ModifiedOnUTC,  ModifiedBy,  CanceledOnUTC,  CanceledBy)
                    VALUES (@PEInternalSubscriptionID, @UserID, @PECustomerID, @PESubscriptionID, @Status,
                            @AmountCents, @TaxCents, @TaxRateThousandPercents, @TotalCents,
                            @CreatedOnUTC, @CreatedBy, @ModifiedOnUTC, @ModifiedBy, @CanceledOnUTC, @CanceledBy)
                    ON DUPLICATE KEY UPDATE
                            UserID = @UserID,
                            PECustomerID = @PECustomerID,
                            PESubscriptionID = @PESubscriptionID,
                            Status = @Status,
                            AmountCents = @AmountCents,
                            TaxCents = @TaxCents,
                            TaxRateThousandPercents = @TaxRateThousandPercents,
                            TotalCents = @TotalCents,
                            ModifiedOnUTC = @ModifiedOnUTC,
                            ModifiedBy = @ModifiedBy,
                            CanceledOnUTC = @CanceledOnUTC,
                            CanceledBy = @CanceledBy
                ";

                var parameters = new List<MySqlParameter>()
                {
                    new MySqlParameter("PEInternalSubscriptionID", record.SubscriptionID),
                    new MySqlParameter("UserID", record.UserID),
                    new MySqlParameter("PECustomerID", record.ParallelEconomyCustomerID),
                    new MySqlParameter("PESubscriptionID", record.ParallelEconomySubscriptionID),
                    new MySqlParameter("Status", record.Status),
                    new MySqlParameter("AmountCents", record.AmountCents),
                    new MySqlParameter("TaxCents", record.TaxCents),
                    new MySqlParameter("TaxRateThousandPercents", record.TaxRateThousandPercents),
                    new MySqlParameter("TotalCents", record.TotalCents),
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
