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
    internal class SqlPaymentRecordProvider : IPaymentRecordProvider
    {
        public readonly MySQLHelper sql;

        public SqlPaymentRecordProvider(MySQLHelper sql)
        {
            this.sql = sql;
        }

        public async Task Delete(Guid userId, Guid subId, Guid paymentId)
        {
            try
            {
                const string query = @"
                    DELETE FROM
                        Payment_PE_Payment
                    WHERE
                        UserID = @UserID
                        AND PEInternalSubscriptionID = @PEInternalSubscriptionID
                        AND PEInternalPaymentID = @PEInternalPaymentID;
                ";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("UserID", userId.ToString()),
                    new MySqlParameter("PEInternalSubscriptionID", subId.ToString()),
                    new MySqlParameter("PEInternalPaymentID", paymentId.ToString()),
                };

                await sql.RunCmd(query, parameters);
            }
            catch (Exception)
            {
            }
        }

        public async Task DeleteAll(Guid userId, Guid subId)
        {
            try
            {
                const string query = @"
                    DELETE FROM
                        Payment_PE_Payment
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

        public async Task<bool> Exists(Guid userId, Guid subId, Guid paymentId)
        {
            var rec = await GetById(userId, subId, paymentId);
            return rec != null;
        }

        public async IAsyncEnumerable<ParallelEconomyPaymentRecord> GetAll()
        {
            const string query = @"
                    SELECT
                        *
                    FROM
                        Payment_PE_Payment
                ";

            using var rdr = await sql.ReturnReader(query);

            while (await rdr.ReadAsync())
            {
                var record = rdr.ParseParallelEconomyPaymentRecord();

                if (record != null)
                    yield return record;
            }
        }

        public async IAsyncEnumerable<ParallelEconomyPaymentRecord> GetAllBySubscriptionId(Guid userId, Guid subId)
        {
            const string query = @"
                    SELECT
                        *
                    FROM
                        Payment_PE_Payment
                    WHERE
                        UserID = @UserID
                        AND PEInternalSubscriptionID = @PEInternalSubscriptionID;
                ";

            var parameters = new MySqlParameter[]
            {
                    new MySqlParameter("UserID", userId.ToString()),
                    new MySqlParameter("PEInternalSubscriptionID", subId.ToString()),
            };

            using var rdr = await sql.ReturnReader(query);

            while (await rdr.ReadAsync())
            {
                var record = rdr.ParseParallelEconomyPaymentRecord();

                if (record != null)
                    yield return record;
            }
        }

        public async IAsyncEnumerable<ParallelEconomyPaymentRecord> GetAllByUserId(Guid userId)
        {
            const string query = @"
                    SELECT
                        *
                    FROM
                        Payment_PE_Payment
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
                var record = rdr.ParseParallelEconomyPaymentRecord();

                if (record != null)
                    yield return record;
            }
        }

        public async IAsyncEnumerable<(Guid userId, Guid subId, Guid paymentId)> GetAllSubscriptionIds()
        {
            const string query = @"
                    SELECT
                        UserID,
                        PEInternalSubscriptionID,
                        PEInternalPaymentID
                    FROM
                        Payment_PE_Payment
                ";

            using var rdr = await sql.ReturnReader(query);

            while (await rdr.ReadAsync())
            {
                var userId = (rdr["UserID"] as string ?? "").ToGuid();
                var subId = (rdr["PEInternalSubscriptionID"] as string ?? "").ToGuid();
                var paymentId = (rdr["PEInternalPaymentID"] as string ?? "").ToGuid();

                if (userId == Guid.Empty) continue;
                if (subId == Guid.Empty) continue;
                if (paymentId == Guid.Empty) continue;

                yield return (userId, subId, paymentId);
            }
        }

        public async Task<ParallelEconomyPaymentRecord?> GetById(Guid userId, Guid subId, Guid paymentId)
        {
            try
            {
                const string query = @"
                    SELECT
                        *
                    FROM
                        Payment_PE_Payment
                    WHERE
                        UserID = @UserID
                        AND PEInternalSubscriptionID = @PEInternalSubscriptionID
                        AND PEInternalPaymentID = @PEInternalPaymentID;
                ";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("UserID", userId.ToString()),
                    new MySqlParameter("PEInternalSubscriptionID", subId.ToString()),
                    new MySqlParameter("PEInternalPaymentID", paymentId.ToString()),
                };

                using var rdr = await sql.ReturnReader(query, parameters);

                if (await rdr.ReadAsync())
                {
                    var record = rdr.ParseParallelEconomyPaymentRecord();

                    return record;
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public Task Save(ParallelEconomyPaymentRecord record)
        {
            return InsertOrUpdate(record);
        }

        private async Task InsertOrUpdate(ParallelEconomyPaymentRecord record)
        {
            try
            {
                const string query = @"
                    INSERT INTO Payment_PE_Payment
                            (PEInternalPaymentID,  PEInternalSubscriptionID,  UserID,  PEPaymentID,  Status,
                             AmountCents,  TaxCents,  TaxRateThousandPercents,  TotalCents,
                             CreatedOnUTC,  CreatedBy,  ModifiedOnUTC,  ModifiedBy,  PaidOnUTC,  PaidThruUTC)
                    VALUES (@PEInternalPaymentID, @PEInternalSubscriptionID, @UserID, @PEPaymentID, @Status,
                            @AmountCents, @TaxCents, @TaxRateThousandPercents, @TotalCents,
                            @CreatedOnUTC, @CreatedBy, @ModifiedOnUTC, @ModifiedBy, @PaidOnUTC, @PaidThruUTC)
                    ON DUPLICATE KEY UPDATE
                            PEInternalSubscriptionID = @PEInternalSubscriptionID,
                            UserID = @UserID,
                            PEPaymentID = @PEPaymentID,
                            Status = @Status,
                            AmountCents = @AmountCents,
                            TaxCents = @TaxCents,
                            TaxRateThousandPercents = @TaxRateThousandPercents,
                            TotalCents = @TotalCents,
                            ModifiedOnUTC = @ModifiedOnUTC,
                            ModifiedBy = @ModifiedBy,
                            PaidOnUTC = @PaidOnUTC,
                            PaidThruUTC = @PaidThruUTC
                ";

                var parameters = new List<MySqlParameter>()
                {
                    new MySqlParameter("PEInternalPaymentID", record.PaymentID),
                    new MySqlParameter("PEInternalSubscriptionID", record.SubscriptionID),
                    new MySqlParameter("UserID", record.UserID),
                    new MySqlParameter("PEPaymentID", record.ParallelEconomyPaymentID),
                    new MySqlParameter("Status", record.Status),
                    new MySqlParameter("AmountCents", record.AmountCents),
                    new MySqlParameter("TaxCents", record.TaxCents),
                    new MySqlParameter("TaxRateThousandPercents", record.TaxRateThousandPercents),
                    new MySqlParameter("TotalCents", record.TotalCents),
                    new MySqlParameter("CreatedOnUTC", record.CreatedOnUTC.ToDateTime()),
                    new MySqlParameter("CreatedBy", record.CreatedBy),
                    new MySqlParameter("ModifiedOnUTC", record.ModifiedOnUTC?.ToDateTime()),
                    new MySqlParameter("ModifiedBy", record.ModifiedBy.Length == 36 ? record.ModifiedBy : null),
                    new MySqlParameter("PaidOnUTC", record.PaidOnUTC?.ToDateTime()),
                    new MySqlParameter("PaidThruUTC", record.PaidThruUTC?.ToDateTime()),
                };

                await sql.RunCmd(query, parameters.ToArray());
            }
            catch (Exception)
            {
            }
        }
    }
}
