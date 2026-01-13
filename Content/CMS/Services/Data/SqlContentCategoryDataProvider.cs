using IT.WebServices.Fragments.Content;
using IT.WebServices.Helpers;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IT.WebServices.Content.CMS.Services.Data
{
    internal class SqlContentCategoryDataProvider
    {
        public readonly MySQLHelper sql;

        public SqlContentCategoryDataProvider(MySQLHelper sql)
        {
            this.sql = sql;
        }

        public async Task Delete(ContentRecord content)
        {
            try
            {
                const string query = @"
                    DELETE FROM
                        CMS_Category
                    WHERE
                        ContentID = @ContentID;
                ";

                foreach (var catId in content.Public.Data.CategoryIds)
                {
                    var parameters = new List<MySqlParameter>()
                    {
                        new MySqlParameter("ContentID", content.Public.ContentID),
                    };

                    await sql.RunCmd(query, parameters.ToArray());
                }
            }
            catch (Exception)
            {
            }
        }

        public async Task<List<string>> GetById(Guid contentId)
        {
            try
            {
                const string query = @"
                    SELECT
                        CategoryID
                    FROM
                        CMS_Category
                    WHERE
                        ContentID = @ContentID;
                ";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("ContentID", contentId.ToString())
                };

                var list = new List<string>();

                using var rdr = await sql.ReturnReader(query, parameters);
                while (await rdr.ReadAsync())
                    list.Add(rdr.GetString(0));

                return list;
            }
            catch (Exception)
            {
                return new();
            }
        }

        public async Task Load(ContentRecord content)
        {
            content.Public.Data.CategoryIds.AddRange(await GetById(content.Public.ContentIDGuid));
        }

        public async Task Update(ContentRecord content)
        {
            await Delete(content);
            await Insert(content);
        }

        private async Task Insert(ContentRecord content)
        {
            try
            {
                const string query = @"
                    INSERT IGNORE INTO CMS_Category (ContentID,  CategoryID)
                                            VALUES (@ContentID, @CategoryID)
                ";

                foreach (var catId in content.Public.Data.CategoryIds)
                {
                    var parameters = new List<MySqlParameter>()
                    {
                        new MySqlParameter("ContentID", content.Public.ContentID),
                        new MySqlParameter("CategoryID", catId),
                    };

                    await sql.RunCmd(query, parameters.ToArray());
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
