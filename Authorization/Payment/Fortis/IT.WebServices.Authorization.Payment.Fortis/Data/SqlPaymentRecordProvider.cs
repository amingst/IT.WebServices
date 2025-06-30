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
                        Payment_Fortis_Payment
                    WHERE
                        UserID = @UserID
                        AND FortisInternalSubscriptionID = @FortisInternalSubscriptionID
                        AND FortisInternalPaymentID = @FortisInternalPaymentID;
                ";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("UserID", userId.ToString()),
                    new MySqlParameter("FortisInternalSubscriptionID", subId.ToString()),
                    new MySqlParameter("FortisInternalPaymentID", paymentId.ToString()),
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
                        Payment_Fortis_Payment
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

        public async Task<bool> Exists(Guid userId, Guid subId, Guid paymentId)
        {
            var rec = await GetById(userId, subId, paymentId);
            return rec != null;
        }

        public async IAsyncEnumerable<FortisPaymentRecord> GetAll()
        {
            const string query = @"
                    SELECT
                        *
                    FROM
                        Payment_Fortis_Payment
                ";

            using var rdr = await sql.ReturnReader(query);

            while (await rdr.ReadAsync())
            {
                var record = rdr.ParseFortisPaymentRecord();

                if (record != null)
                    yield return record;
            }
        }

        public async IAsyncEnumerable<FortisPaymentRecord> GetAllBySubscriptionId(Guid userId, Guid subId)
        {
            const string query = @"
                    SELECT
                        *
                    FROM
                        Payment_Fortis_Payment
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

            while (await rdr.ReadAsync())
            {
                var record = rdr.ParseFortisPaymentRecord();

                if (record != null)
                    yield return record;
            }
        }

        public async IAsyncEnumerable<FortisPaymentRecord> GetAllByUserId(Guid userId)
        {
            const string query = @"
                    SELECT
                        *
                    FROM
                        Payment_Fortis_Payment
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
                var record = rdr.ParseFortisPaymentRecord();

                if (record != null)
                    yield return record;
            }
        }

        public async IAsyncEnumerable<(Guid userId, Guid subId, Guid paymentId)> GetAllSubscriptionIds()
        {
            const string query = @"
                    SELECT
                        UserID,
                        FortisInternalSubscriptionID,
                        FortisInternalPaymentID
                    FROM
                        Payment_Fortis_Payment
                ";

            using var rdr = await sql.ReturnReader(query);

            while (await rdr.ReadAsync())
            {
                var userId = (rdr["UserID"] as string ?? "").ToGuid();
                var subId = (rdr["FortisInternalSubscriptionID"] as string ?? "").ToGuid();
                var paymentId = (rdr["FortisInternalPaymentID"] as string ?? "").ToGuid();

                if (userId == Guid.Empty) continue;
                if (subId == Guid.Empty) continue;
                if (paymentId == Guid.Empty) continue;

                yield return (userId, subId, paymentId);
            }
        }

        public async Task<FortisPaymentRecord?> GetById(Guid userId, Guid subId, Guid paymentId)
        {
            try
            {
                const string query = @"
                    SELECT
                        *
                    FROM
                        Payment_Fortis_Payment
                    WHERE
                        UserID = @UserID
                        AND FortisInternalSubscriptionID = @FortisInternalSubscriptionID
                        AND FortisInternalPaymentID = @FortisInternalPaymentID;
                ";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("UserID", userId.ToString()),
                    new MySqlParameter("FortisInternalSubscriptionID", subId.ToString()),
                    new MySqlParameter("FortisInternalPaymentID", paymentId.ToString()),
                };

                using var rdr = await sql.ReturnReader(query, parameters);

                if (await rdr.ReadAsync())
                {
                    var record = rdr.ParseFortisPaymentRecord();

                    return record;
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public Task Save(FortisPaymentRecord record)
        {
            return InsertOrUpdate(record);
        }

        private async Task InsertOrUpdate(FortisPaymentRecord record)
        {
            try
            {
                const string query = @"
                    INSERT INTO Payment_Fortis_Payment
                            (FortisInternalPaymentID,  FortisInternalSubscriptionID,  UserID,  FortisPaymentID,  Status,
                             AmountCents,  TaxCents,  TaxRateThousandPercents,  TotalCents,
                             CreatedOnUTC,  CreatedBy,  ModifiedOnUTC,  ModifiedBy,  PaidOnUTC,  PaidThruUTC)
                    VALUES (@FortisInternalPaymentID, @FortisInternalSubscriptionID, @UserID, @FortisPaymentID, @Status,
                            @AmountCents, @TaxCents, @TaxRateThousandPercents, @TotalCents,
                            @CreatedOnUTC, @CreatedBy, @ModifiedOnUTC, @ModifiedBy, @PaidOnUTC, @PaidThruUTC)
                    ON DUPLICATE KEY UPDATE
                            FortisInternalSubscriptionID = @FortisInternalSubscriptionID,
                            UserID = @UserID,
                            FortisPaymentID = @FortisPaymentID,
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
                    new MySqlParameter("FortisInternalPaymentID", record.PaymentID),
                    new MySqlParameter("FortisInternalSubscriptionID", record.SubscriptionID),
                    new MySqlParameter("UserID", record.UserID),
                    new MySqlParameter("FortisPaymentID", record.FortisPaymentID),
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
