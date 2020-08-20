using MySql.Data.MySqlClient;
using Seeder.Configuration;
using Seeder.Generator.Mysql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seeder.Examples.Mysql
{
    class Program
    {
        static void Main(string[] args)
        {
            var defaultCfg = new DatabaseConfiguration()
            {
                Tables = new List<TableConfiguration>()
                {
                    TableConfiguration.CreateDefault("single_key_example")
                }
            };

            using (var conn = new MySqlConnection("Server=localhost;Database=seederexample;Uid=root;Pwd=123456;"))
            {
                conn.Open();
                var sql = MysqlGenerator.GenerateSql(defaultCfg, conn);
            }
        }
    }
}
