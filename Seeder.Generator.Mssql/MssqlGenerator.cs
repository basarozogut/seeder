using System.Data;
using System.Linq;
using System.Text;
using Seeder.Configuration;
using Seeder.Generator.Interfaces;

namespace Seeder.Generator.Mssql
{
    public sealed class MssqlGenerator : ISqlGenerator
    {
        private readonly DatabaseConfiguration _configuration;
        private readonly IDataAccess _dataAccess;

        /// <summary>
        /// Create a seed script generator for a MSSQL database.
        /// </summary>
        /// <param name="configuration">The database configuration</param>
        /// <param name="dataAccess">The data access layer for MSSQL database</param>
        public MssqlGenerator(DatabaseConfiguration configuration, IDataAccess dataAccess)
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
                var generatedSql = new TableMergeGenerator(tableConfiguration, databaseColumns, _dataAccess).Generate();
                sb.Append(generatedSql);
            }

            return sb.ToString();
        }
    }
}
