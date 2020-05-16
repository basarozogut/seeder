using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Seeder.Configuration;
using Seeder.Generator.DataObjects;
using Seeder.Generator.Interfaces;
using Seeder.Generator.Mssql.SqlStringBuilder;

namespace Seeder.Generator.Mssql
{
    public sealed class MssqlGenerator : ISqlGenerator
    {
        private readonly DatabaseConfiguration _configuration;
        private readonly IDataAccess _dataAccess;
        private readonly ISqlStringBuilderFactory _sqlStringBuilderFactory;

        public MssqlGenerator(DatabaseConfiguration configuration, IDataAccess dataAccess, bool prettyString = true)
        {
            _configuration = configuration;
            _dataAccess = dataAccess;

            if (prettyString)
                _sqlStringBuilderFactory = new SqlPrettyStringBuilderFactory();
            else
                _sqlStringBuilderFactory = new SqlCompactStringBuilderFactory();
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
                else
                {
                    MakeSecurityCheckForTable(tableConfiguration, databaseColumns);
                }
                var generatedSql = new TableMergeGenerator(tableConfiguration, databaseColumns, _dataAccess, _sqlStringBuilderFactory).Generate();
                sb.Append(generatedSql);
            }

            return sb.ToString();
        }

        private void MakeSecurityCheckForTable(TableConfiguration tableConfiguration, List<DatabaseColumn> databaseColumns)
        {
            var databaseColumnNames = databaseColumns.Select(r => r.ColumnName).ToList();
            foreach (var column in tableConfiguration.Columns)
            {
                if (!databaseColumnNames.Contains(column))
                    throw new SqlGeneratorException($"Column not found in database! ({column})");
            }
        }

        public void Dispose()
        {
            _dataAccess?.Dispose();
        }
    }
}
