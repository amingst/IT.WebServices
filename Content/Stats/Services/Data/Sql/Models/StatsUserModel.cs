using IT.WebServices.Helpers;
using MySql.Data.MySqlClient;
using System;
using System.Threading.Tasks;

namespace IT.WebServices.Content.Stats.Services.Data.Sql.Models
{
    public class StatsUserModel
    {
        public long Likes { get; set; } = 0;
        public long Saves { get; set; } = 0;
        public long Shares { get; set; } = 0;
        public long Views { get; set; } = 0;

        public static async Task<StatsUserModel> GetById(MySQLHelper sql, Guid userId)
        {
            var model = new StatsUserModel();

            try
            {
                const string query = @"
                    SELECT
                        Likes,
                        Saves,
                        Shares,
                        Views
                    FROM
                        Stats_User
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
                    model.Likes = rdr.GetInt64(0);
                    model.Saves = rdr.GetInt64(1);
                    model.Shares = rdr.GetInt64(2);
                    model.Views = rdr.GetInt64(3);
                }
            }
            catch { }

            return model;
        }

        public static async Task DecrementLikes(MySQLHelper sql, Guid userId)
        {
            if (userId == Guid.Empty)
                return;

            try
            {
                const string query = @"
                    INSERT INTO Stats_User
                            (UserID,  Likes)
                    VALUES (@UserID,  0)
                    ON DUPLICATE KEY UPDATE
                            Likes = Likes - 1
                ";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("UserID", userId.ToString()),
                };

                await sql.RunCmd(query, parameters);
            }
            catch { }
        }

        public static async Task DecrementSaves(MySQLHelper sql, Guid userId)
        {
            if (userId == Guid.Empty)
                return;

            try
            {
                const string query = @"
                    INSERT INTO Stats_User
                            (UserID,  Saves)
                    VALUES (@UserID,  0)
                    ON DUPLICATE KEY UPDATE
                            Saves = Saves - 1
                ";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("UserID", userId.ToString()),
                };

                await sql.RunCmd(query, parameters);
            }
            catch { }
        }

        public static async Task IncrementLikes(MySQLHelper sql, Guid userId)
        {
            if (userId == Guid.Empty)
                return;

            try
            {
                const string query = @"
                    INSERT INTO Stats_User
                            (UserID,  Likes)
                    VALUES (@UserID,  1)
                    ON DUPLICATE KEY UPDATE
                            Likes = Likes + 1
                ";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("UserID", userId.ToString()),
                };

                await sql.RunCmd(query, parameters);
            }
            catch { }
        }

        public static async Task IncrementSaves(MySQLHelper sql, Guid userId)
        {
            if (userId == Guid.Empty)
                return;

            try
            {
                const string query = @"
                    INSERT INTO Stats_User
                            (UserID,  Saves)
                    VALUES (@UserID,  1)
                    ON DUPLICATE KEY UPDATE
                            Saves = Saves + 1
                ";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("UserID", userId.ToString()),
                };

                await sql.RunCmd(query, parameters);
            }
            catch { }
        }

        public static async Task IncrementShares(MySQLHelper sql, Guid userId)
        {
            if (userId == Guid.Empty)
                return;

            try
            {
                const string query = @"
                    INSERT INTO Stats_User
                            (UserID,  Shares)
                    VALUES (@UserID,  1)
                    ON DUPLICATE KEY UPDATE
                            Shares = Shares + 1
                ";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("UserID", userId.ToString()),
                };

                await sql.RunCmd(query, parameters);
            }
            catch { }
        }

        public static async Task IncrementViews(MySQLHelper sql, Guid userId)
        {
            if (userId == Guid.Empty)
                return;

            try
            {
                const string query = @"
                    INSERT INTO Stats_User
                            (UserID,  Views)
                    VALUES (@UserID,  1)
                    ON DUPLICATE KEY UPDATE
                            Views = Views + 1
                ";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("UserID", userId.ToString()),
                };

                await sql.RunCmd(query, parameters);
            }
            catch { }
        }
    }
}
