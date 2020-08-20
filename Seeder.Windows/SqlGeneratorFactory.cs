using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Seeder.Configuration;
using Seeder.Generator.Interfaces;
using Seeder.Generator.Mssql;
using Seeder.Generator.Mysql;

namespace Seeder.Windows
{
    class SqlGeneratorFactory : ISqlGeneratorFactory
    {
        public ISqlGenerator CreateSqlGenerator(DatabaseConfiguration databaseConfiguration, string databaseType, string connectionString, out DbConnection dbConnection)
        {
            if (databaseType == "mssql")
            {
                var connection = new System.Data.SqlClient.SqlConnection(connectionString);
                dbConnection = connection;
                return new MssqlGenerator(databaseConfiguration, new MssqlDataAccess(connection));
            }
            else if (databaseType == "mysql")
            {
                var connection = new MySqlConnection(connectionString);
                dbConnection = connection;
                return new MysqlGenerator(databaseConfiguration, new MysqlDataAccess(connection));
            }
            else
            {
                throw new SeederException($"Unknown database type: {databaseType}");
            }
        }
    }
}
