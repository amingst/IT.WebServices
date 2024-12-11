using IT.WebServices.Helpers;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT.WebServices.Content.Stats.Services.Data.Sql.Models
{
    public class StatsViewsModel
    {
        public DateTime ViewedOnUTC { get; set; }

        public static async Task LogView(MySQLHelper sql, Guid contentId, Guid userId)
        {
            try
            {
                if (contentId == Guid.Empty)
                    return;

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
            catch { }
        }
    }
}
