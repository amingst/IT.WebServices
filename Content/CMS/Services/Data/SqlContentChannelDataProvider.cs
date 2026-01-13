using IT.WebServices.Fragments.Content;
using IT.WebServices.Helpers;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IT.WebServices.Content.CMS.Services.Data
{
    internal class SqlContentChannelDataProvider
    {
        public readonly MySQLHelper sql;

        public SqlContentChannelDataProvider(MySQLHelper sql)
        {
            this.sql = sql;
        }

        public async Task Delete(ContentRecord content)
        {
            try
            {
                const string query = @"
                    DELETE FROM
                        CMS_Channel
                    WHERE
                        ContentID = @ContentID;
                ";

                foreach (var chanId in content.Public.Data.ChannelIds)
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
                        ChannelID
                    FROM
                        CMS_Channel
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
            content.Public.Data.ChannelIds.AddRange(await GetById(content.Public.ContentIDGuid));
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
                    INSERT IGNORE INTO CMS_Channel (ContentID,  ChannelID)
                                            VALUES (@ContentID, @ChannelID)
                ";

                foreach (var chanId in content.Public.Data.ChannelIds)
                {
                    var parameters = new List<MySqlParameter>()
                    {
                        new MySqlParameter("ContentID", content.Public.ContentID),
                        new MySqlParameter("ChannelID", chanId),
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
