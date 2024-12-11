using IT.WebServices.Helpers;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT.WebServices.Content.Stats.Services.Data.Sql.Models
{
    public class StatsSharesModel
    {
        public DateTime ViewedOnUTC { get; set; }

        public static async Task LogShare(MySQLHelper sql, Guid contentId, Guid userId)
        {
            if (contentId == Guid.Empty)
                return;

            try
            {
                const string query = @"
                    INSERT INTO Stats_Shares
                            (ContentID,  UserID,  SharedOnUTC)
                    VALUES (@ContentID, @UserID,  UTC_TIMESTAMP())
                ";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("ContentID", contentId.ToString()),
                    new MySqlParameter("UserID", userId.ToString()),
                };

                await sql.RunCmd(query, parameters);
            }
            catch { }
        }
    }
}
