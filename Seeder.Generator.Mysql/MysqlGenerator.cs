using System.Data;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using Seeder.Configuration;
using Seeder.Generator.Interfaces;

namespace Seeder.Generator.Mysql
{
    public sealed class MysqlGenerator : ISqlGenerator
    {
        private readonly DatabaseConfiguration _configuration;
        private readonly IDataAccess _dataAccess;

        /// <summary>
        /// Create a seed script generator for a MYSQL database.
        /// </summary>
        /// <param name="configuration">The database configuration</param>
        /// <param name="dataAccess">The data access layer for MYSQL database</param>
        public MysqlGenerator(DatabaseConfiguration configuration, IDataAccess dataAccess)
        {
            _configuration = configuration;
            _dataAccess = dataAccess;
        }

        public string GenerateSql()
        {
            var sb = new StringBuilder();

            foreach (var tableConfiguration in _configuration.Tables)
            {
                var databaseColumns = _dataAccess.GetColumnStructureFromDatabase(tableConfiguration);
                if (tableConfiguration.AutoFindColumns)
                {
                    tableConfiguration.Columns = databaseColumns.Select(r => r.ColumnName).ToList();
                }
                var generatedSql = new UpsertGenerator(tableConfiguration, databaseColumns, _dataAccess).Generate();
                sb.Append(generatedSql);
            }

            return sb.ToString().Trim();
        }

        public static string GenerateSql(DatabaseConfiguration databaseConfiguration, MySqlConnection mysqlConnection)
        {
            return new MysqlGenerator(databaseConfiguration, new MysqlDataAccess(mysqlConnection)).GenerateSql();
        }
    }
}
