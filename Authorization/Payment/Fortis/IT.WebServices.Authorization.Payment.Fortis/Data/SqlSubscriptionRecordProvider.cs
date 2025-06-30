using IT.WebServices.Authorization.Payment.Fortis.Helpers;
using IT.WebServices.Fragments.Authentication;
using IT.WebServices.Fragments.Authorization.Payment.Fortis;
using IT.WebServices.Fragments.Content;
using IT.WebServices.Fragments.Generic;
using IT.WebServices.Helpers;
using Microsoft.AspNetCore.Components;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IT.WebServices.Authorization.Payment.Fortis.Data
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
                        Payment_Fortis_Subscription
                    WHERE
                        UserID = @UserID
                        AND FortisInternalSubscriptionID = @FortisInternalSubscriptionID;
                ";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("UserID", userId.ToString()),
                    new MySqlParameter("FortisInternalSubscriptionID", subId.ToString()),
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

        public async IAsyncEnumerable<FortisSubscriptionRecord> GetAll()
        {
            const string query = @"
                    SELECT
                        *
                    FROM
                        Payment_Fortis_Subscription
                ";

            using var rdr = await sql.ReturnReader(query);

            while (await rdr.ReadAsync())
            {
                var record = rdr.ParseFortisSubscriptionRecord();

                if (record != null)
                    yield return record;
            }
        }

        public async IAsyncEnumerable<FortisSubscriptionRecord> GetAllByUserId(Guid userId)
        {
            const string query = @"
                    SELECT
                        *
                    FROM
                        Payment_Fortis_Subscription
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
                var record = rdr.ParseFortisSubscriptionRecord();

                if (record != null)
                    yield return record;
            }
        }

        public async IAsyncEnumerable<(Guid userId, Guid subId)> GetAllSubscriptionIds()
        {
            const string query = @"
                    SELECT
                        UserID,
                        FortisInternalSubscriptionID
                    FROM
                        Payment_Fortis_Subscription
                ";

            using var rdr = await sql.ReturnReader(query);

            while (await rdr.ReadAsync())
            {
                var userId = (rdr["UserID"] as string ?? "").ToGuid();
                var subId = (rdr["FortisInternalSubscriptionID"] as string ?? "").ToGuid();

                if (userId == Guid.Empty) continue;
                if (subId == Guid.Empty) continue;

                yield return (userId, subId);
            }
        }

        public async Task<FortisSubscriptionRecord?> GetById(Guid userId, Guid subId)
        {
            try
            {
                const string query = @"
                    SELECT
                        *
                    FROM
                        Payment_Fortis_Subscription
                    WHERE
                        UserID = @UserID
                        AND FortisInternalSubscriptionID = @FortisInternalSubscriptionID;
                ";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("UserID", userId.ToString()),
                    new MySqlParameter("FortisInternalSubscriptionID", subId.ToString()),
                };

                using var rdr = await sql.ReturnReader(query, parameters);

                if (await rdr.ReadAsync())
                {
                    var record = rdr.ParseFortisSubscriptionRecord();

                    return record;
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public Task Save(FortisSubscriptionRecord record)
        {
            return InsertOrUpdate(record);
        }

        private async Task InsertOrUpdate(FortisSubscriptionRecord record)
        {
            try
            {
                const string query = @"
                    INSERT INTO Payment_Fortis_Subscription
                            (FortisInternalSubscriptionID,  UserID,  FortisCustomerID,  FortisSubscriptionID,  Status,
                             AmountCents,  TaxCents,  TaxRateThousandPercents,  TotalCents,
                             CreatedOnUTC,  CreatedBy,  ModifiedOnUTC,  ModifiedBy,  CanceledOnUTC,  CanceledBy)
                    VALUES (@FortisInternalSubscriptionID, @UserID, @FortisCustomerID, @FortisSubscriptionID, @Status,
                            @AmountCents, @TaxCents, @TaxRateThousandPercents, @TotalCents,
                            @CreatedOnUTC, @CreatedBy, @ModifiedOnUTC, @ModifiedBy, @CanceledOnUTC, @CanceledBy)
                    ON DUPLICATE KEY UPDATE
                            UserID = @UserID,
                            FortisCustomerID = @FortisCustomerID,
                            FortisSubscriptionID = @FortisSubscriptionID,
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
                    new MySqlParameter("FortisInternalSubscriptionID", record.SubscriptionID),
                    new MySqlParameter("UserID", record.UserID),
                    new MySqlParameter("FortisCustomerID", record.FortisCustomerID),
                    new MySqlParameter("FortisSubscriptionID", record.FortisSubscriptionID),
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
