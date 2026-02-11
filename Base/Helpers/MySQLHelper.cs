using IT.WebServices.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace IT.WebServices.Helpers
{
    public class MySQLHelper
    {
        public readonly string connString;

        public MySQLHelper(IOptions<AppSettings> settings)
        {
            connString = settings.Value.MySQLConn ?? "";
        }

        public async Task<DbDataReader> ReturnReader(string query, params MySqlParameter[] pList)
        {
            var conn = new MySqlConnection(connString);
            await conn.OpenAsync();
            using var cmd = new MySqlCommand(query, conn);

            cmd.CommandTimeout = 60000;

            if (pList != null)
                cmd.Parameters.AddRange(pList);

            return await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        }

        public async Task RunCmd(string query, params MySqlParameter[] pList)
        {
            using var conn = new MySqlConnection(connString);
            await conn.OpenAsync();
            using var cmd = new MySqlCommand(query, conn);

            cmd.CommandTimeout = 60000;

            if (pList != null)
                cmd.Parameters.AddRange(pList);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
