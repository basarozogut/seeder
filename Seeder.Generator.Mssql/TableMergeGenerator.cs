using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
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
        private readonly IDataToValueConverter dataToValueConverter = new MssqlDataToValueConverter();

        public TableMergeGenerator(
            TableConfiguration tableConfiguration,
            List<DatabaseColumn> databaseColumns,
            IDataAccess dataAccess)
        {
            _tableConfiguration = tableConfiguration;
            _databaseColumns = databaseColumns;
            _dataAccess = dataAccess;
        }

        public string Generate()
        {
            var rows = _dataAccess.GetDataForTable(_tableConfiguration, _databaseColumns);

            var sql = new StringBuilder();

            sql.AppendLine($"-- Seed for [{_tableConfiguration.TableName}]");

            sql.AppendLine($"MERGE {_tableConfiguration.TableName} AS t USING (VALUES");
            foreach (var row in rows)
            {
                sql.Append("(");
                foreach (var column in row.Data)
                {
                    sql.Append(dataToValueConverter.Convert(column));
                    if (column != row.Data.Last())
                        sql.Append(",");
                }
                sql.Append(")");

                if (row != rows.Last())
                    sql.AppendLine(",");
                else
                    sql.AppendLine("");
            }
            var idQuery = _tableConfiguration.IdColumns.Select(r => $"(s.{r} = t.{r})");
            sql.AppendLine($") AS s ({string.Join(", ", _tableConfiguration.Columns)}) ON {string.Join(" AND ", idQuery)}");

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

            sql.AppendLine(";");

            return sql.ToString();
        }

        private void GenerateUpdate(StringBuilder sql)
        {
            sql.AppendLine("WHEN MATCHED");
            sql.AppendLine("THEN UPDATE SET");
            foreach (var column in GetAllColumnsExceptIdColumns())
            {
                sql.Append($"t.{column} = s.{column}");
                if (column != _tableConfiguration.Columns.Last())
                    sql.AppendLine(",");
                else
                    sql.AppendLine("");
            }
        }

        private IEnumerable<string> GetAllColumnsExceptIdColumns()
        {
            return _tableConfiguration.Columns.Where(r => !_tableConfiguration.IdColumns.Contains(r));
        }

        private void GenerateInsert(StringBuilder sql)
        {
            sql.AppendLine("WHEN NOT MATCHED BY TARGET");
            sql.AppendLine($"THEN INSERT ({string.Join(", ", _tableConfiguration.Columns)})");
            sql.AppendLine($"VALUES ({string.Join(", ", _tableConfiguration.Columns.Select(r => $"s.{r}"))})");
        }

        private void GenerateDelete(StringBuilder sql)
        {
            sql.AppendLine("WHEN NOT MATCHED BY SOURCE");
            sql.AppendLine("THEN DELETE");
        }
    }
}
