using Seeder.Configuration;
using Seeder.Generator.DataObjects;
using Seeder.Generator.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Seeder.Generator.Mysql
{
    class UpsertGenerator
    {
        private readonly TableConfiguration _tableConfiguration;
        private readonly List<DatabaseColumn> _databaseColumns;
        private readonly IDataAccess _dataAccess;
        private readonly ISqlStringBuilderFactory _sqlStringBuilderFactory;

        public UpsertGenerator(
            TableConfiguration tableConfiguration,
            List<DatabaseColumn> databaseColumns,
            IDataAccess dataAccess,
            ISqlStringBuilderFactory sqlStringBuilderFactory)
        {
            _tableConfiguration = tableConfiguration;
            _databaseColumns = databaseColumns;
            _dataAccess = dataAccess;
            _sqlStringBuilderFactory = sqlStringBuilderFactory;
        }

        public string Generate()
        {
            var rows = _dataAccess.GetDataForTable(_tableConfiguration, _databaseColumns);

            var sql = _sqlStringBuilderFactory.CreateSqlStringBuilder();

            sql.AppendCommentLine($"-- Seed for [{_tableConfiguration.TableName}]");

            // TODO write statement generator.

            return sql.ToString();
        }

        private void WriteValues(ISqlStringBuilder sql, List<DatabaseRow> rows)
        {
            foreach (var row in rows)
            {
                sql.Append("(");
                foreach (var column in row.Data)
                {
                    sql.Append(GetFormattedValueForColumn(column));
                    if (column != row.Data.Last())
                        sql.Append(",");
                }
                sql.Append(")");

                if (row != rows.Last())
                    sql.AppendLine(",");
                else
                    sql.AppendLine();
            }
        }

        private string GetFormattedValueForColumn(DatabaseData data)
        {
            if (data.Value == null)
                return "NULL";

            if (data.Column.DataType.StartsWith("varchar") ||
                data.Column.DataType.StartsWith("nvarchar"))
                return $"'{data.Value}'";

            return data.Value.ToString();
        }
    }
}
