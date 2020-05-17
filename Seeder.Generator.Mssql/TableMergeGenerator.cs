using System.Collections.Generic;
using System.Data;
using System.Linq;
using Seeder.Configuration;
using Seeder.Generator.DataObjects;
using Seeder.Generator.Interfaces;

namespace Seeder.Generator.Mssql
{
    /// <summary>
    /// Helper class for generating MSSQL merge statements.
    /// </summary>
    sealed class TableMergeGenerator
    {
        private readonly TableConfiguration _tableConfiguration;
        private readonly List<DatabaseColumn> _databaseColumns;
        private readonly IDataAccess _dataAccess;
        private readonly ISqlStringBuilderFactory _sqlStringBuilderFactory;

        public TableMergeGenerator(
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

            sql.AppendLine($"MERGE {_tableConfiguration.TableName} AS t USING (VALUES");
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
            sql.AppendLine($") AS s ({string.Join(", ", _tableConfiguration.Columns)}) ON (s.{_tableConfiguration.IdColumn} = t.{_tableConfiguration.IdColumn})");

            if (_tableConfiguration.EnableUpdate)
            {
                GenerateUpdate(sql);
            }

            if (_tableConfiguration.EnableInsert)
            {
                GenerateInsert(sql);
            }

            if (_tableConfiguration.EnableDelete)
            {
                GenerateDelete(sql);
            }

            sql.EndStatement();

            return sql.ToString();
        }

        private void GenerateUpdate(ISqlStringBuilder sql)
        {
            sql.AppendLine("WHEN MATCHED");
            sql.AppendLine("THEN UPDATE SET");
            foreach (var column in _tableConfiguration.Columns.Where(r => r != _tableConfiguration.IdColumn))
            {
                sql.Append($"t.{column} = s.{column}");
                if (column != _tableConfiguration.Columns.Last())
                    sql.AppendLine(",");
                else
                    sql.AppendLine("");
            }
        }

        private void GenerateInsert(ISqlStringBuilder sql)
        {
            sql.AppendLine("WHEN NOT MATCHED BY TARGET");
            sql.AppendLine($"THEN INSERT({string.Join(", ", _tableConfiguration.Columns)})");
            sql.AppendLine($"VALUES({string.Join(", ", _tableConfiguration.Columns.Select(r => $"s.{r}"))})");
        }

        private void GenerateDelete(ISqlStringBuilder sql)
        {
            sql.AppendLine("WHEN NOT MATCHED BY SOURCE");
            sql.Append("THEN DELETE");
        }

        private string GetFormattedValueForColumn(DatabaseData data)
        {
            if (data.Value == null)
                return "NULL";

            if (data.Column.DataType == "nvarchar")
                return $"'{data.Value}'";

            if (data.Column.DataType == "bit")
                return ((bool)data.Value) ? "1" : "0";

            if (data.Column.DataType == "datetime") // conversion not supported for now
                return "NULL"; // TODO make conversion

            return data.Value.ToString();
        }
    }
}
