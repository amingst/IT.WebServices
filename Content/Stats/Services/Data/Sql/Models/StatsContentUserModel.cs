using IT.WebServices.Helpers;
using MySql.Data.MySqlClient;
using System;
using System.Threading.Tasks;

namespace IT.WebServices.Content.Stats.Services.Data.Sql.Models
{
    public class StatsContentUserModel
    {
        public DateTime? LikedOnUTC { get; set; }
        public DateTime? SavedOnUTC { get; set; }
        public int Shares { get; set; }
        public int Views { get; set; }

        #region Likes
        public static async Task SetLiked(MySQLHelper sql, Guid contentId, Guid userId)
        {
            if (contentId == Guid.Empty)
                return;
            if (userId == Guid.Empty)
                return;

            try
            {
                const string query = @"
                    INSERT INTO Stats_ContentUser
                            (ContentID,  UserID,  LikedOnUTC)
                    VALUES (@ContentID, @UserID,  UTC_TIMESTAMP())
                    ON DUPLICATE KEY UPDATE
                            LikedOnUTC = UTC_TIMESTAMP()
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

        public static async Task SetUnLiked(MySQLHelper sql, Guid contentId, Guid userId)
        {
            if (contentId == Guid.Empty)
                return;
            if (userId == Guid.Empty)
                return;

            try
            {
                const string query = @"
                    INSERT INTO Stats_ContentUser
                            (ContentID,  UserID,  LikedOnUTC)
                    VALUES (@ContentID, @UserID,  NULL)
                    ON DUPLICATE KEY UPDATE
                            LikedOnUTC = NULL
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
        #endregion

        #region Views
        public static async Task LogProgress(MySQLHelper sql, Guid contentId, Guid userId, float progress)
        {
            if (contentId == Guid.Empty)
                return;
            if (userId == Guid.Empty)
                return;

            try
            {
                const string query = @"
                    INSERT INTO Stats_ContentUser
                            (ContentID,  UserID,  Progress,  ProgressUpdatedOnUTC)
                    VALUES (@ContentID, @UserID, @Progress,  UTC_TIMESTAMP())
                    ON DUPLICATE KEY UPDATE
                            Progress = @Progress,
                            ProgressUpdatedOnUTC = UTC_TIMESTAMP()
                ";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("ContentID", contentId.ToString()),
                    new MySqlParameter("UserID", userId.ToString()),
                    new MySqlParameter("Progress", progress),
                };

                await sql.RunCmd(query, parameters);
            }
            catch { }
        }

        public static async Task ClearProgress(MySQLHelper sql, Guid contentId, Guid userId)
        {
            if (contentId == Guid.Empty)
                return;
            if (userId == Guid.Empty)
                return;

            try
            {
                const string query = @"
                    INSERT INTO Stats_ContentUser
                            (ContentID,  UserID,  Progress,  ProgressUpdatedOnUTC)
                    VALUES (@ContentID, @UserID,  NULL,      NULL)
                    ON DUPLICATE KEY UPDATE
                            Progress = NULL,
                            ProgressUpdatedOnUTC = NULL
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
        #endregion

        #region Saved
        public static async Task SetSaved(MySQLHelper sql, Guid contentId, Guid userId)
        {
            if (contentId == Guid.Empty)
                return;
            if (userId == Guid.Empty)
                return;

            try
            {
                const string query = @"
                    INSERT INTO Stats_ContentUser
                            (ContentID,  UserID,  SavedOnUTC)
                    VALUES (@ContentID, @UserID,  UTC_TIMESTAMP())
                    ON DUPLICATE KEY UPDATE
                            SavedOnUTC = UTC_TIMESTAMP()
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

        public static async Task SetUnSaved(MySQLHelper sql, Guid contentId, Guid userId)
        {
            if (contentId == Guid.Empty)
                return;
            if (userId == Guid.Empty)
                return;

            try
            {
                const string query = @"
                    INSERT INTO Stats_ContentUser
                            (ContentID,  UserID,  SavedOnUTC)
                    VALUES (@ContentID, @UserID,  NULL)
                    ON DUPLICATE KEY UPDATE
                            SavedOnUTC = NULL
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
        #endregion

        #region Shared
        public static async Task LogShare(MySQLHelper sql, Guid contentId, Guid userId)
        {
            if (contentId == Guid.Empty)
                return;
            if (userId == Guid.Empty)
                return;

            try
            {
                const string query = @"
                    INSERT INTO Stats_ContentUser
                            (ContentID,  UserID,  NumberOfShares)
                    VALUES (@ContentID, @UserID,  1)
                    ON DUPLICATE KEY UPDATE
                            NumberOfShares = NumberOfShares + 1
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
        #endregion

        #region Views
        public static async Task LogView(MySQLHelper sql, Guid contentId, Guid userId)
        {
            if (contentId == Guid.Empty)
                return;
            if (userId == Guid.Empty)
                return;

            try
            {
                const string query = @"
                    INSERT INTO Stats_ContentUser
                            (ContentID,  UserID,  ViewedLastOnUTC,  NumberOfViews)
                    VALUES (@ContentID, @UserID,  UTC_TIMESTAMP(),  1)
                    ON DUPLICATE KEY UPDATE
                            NumberOfViews = NumberOfViews + 1
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
        #endregion
    }
}
