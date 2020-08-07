using Seeder.Configuration;
using Seeder.Generator.DataObjects;
using Seeder.Generator.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seeder.Generator.Mysql
{
    class UpsertGenerator
    {
        private readonly TableConfiguration _tableConfiguration;
        private readonly List<DatabaseColumn> _databaseColumns;
        private readonly IDataAccess _dataAccess;
        private readonly IDataToValueConverter _dataToValueConverter = new MysqlDataToValueConverter();

        public UpsertGenerator(
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

            if (_tableConfiguration.EnableInsert && _tableConfiguration.EnableUpdate)
            {
                foreach (var row in rows)
                {
                    WriteInsertOrUpdate(sql, row);
                }
            }
            else if (_tableConfiguration.EnableInsert)
            {
                WriteInsert(sql, rows);
            }
            else if (_tableConfiguration.EnableUpdate)
            {
                foreach (var row in rows)
                {
                    WriteUpdate(sql, row);
                }
            }

            if (_tableConfiguration.EnableDelete)
            {
                foreach (var row in rows)
                {
                    WriteDelete(sql, row);
                }
            }

            return sql.ToString();
        }

        private void WriteInsert(StringBuilder sql, List<DatabaseRow> rows)
        {
            /**
             * INSERT INTO tablename
                (id, val1, val2)
            VALUES
                (1, 'a', 'b'),
                (2, 'x', 'y')
             */

            sql.AppendLine($"INSERT INTO {_tableConfiguration.TableName}");
            sql.AppendLine($"({string.Join(", ", _tableConfiguration.Columns)})");
            sql.AppendLine("VALUES");
            var generatedValues = rows.Select(r => GenerateValuesForRow(r)).ToList();
            foreach (var generatedValue in generatedValues)
            {
                sql.Append(generatedValue);
                if (generatedValue != generatedValues.Last())
                    sql.AppendLine(",");
                else
                    sql.Append("");
            }
            sql.AppendLine(";");
        }

        private void WriteInsertOrUpdate(StringBuilder sql, DatabaseRow row)
        {
            /**
            * INSERT INTO tablename
               (id, val1, val2)
             VALUES
               (1, 'a', 'b')
             ON DUPLICATE KEY UPDATE
                val1 = @val1,
                val2 = @val2
            */

            sql.AppendLine($"INSERT INTO {_tableConfiguration.TableName}");
            sql.AppendLine($"({string.Join(", ", _tableConfiguration.Columns)})");
            sql.AppendLine("VALUES");
            sql.AppendLine(GenerateValuesForRow(row));
            sql.AppendLine("ON DUPLICATE KEY UPDATE");
            foreach (var data in row.Data.Where(r => !r.Column.IdColumn))
            {
                sql.Append($"{data.Column.ColumnName} = {_dataToValueConverter.Convert(data)}");

                if (data != row.Data.Last())
                    sql.AppendLine(",");
                else
                    sql.Append("");
            }
            sql.AppendLine(";");
        }

        private void WriteUpdate(StringBuilder sql, DatabaseRow row)
        {
            /**
            UPDATE tablename
            SET
                val1 = @val1,
                val2 = @val2
            WHERE
                id = @id
            */

            sql.AppendLine($"UPDATE {_tableConfiguration.TableName}");
            sql.AppendLine("SET");
            foreach (var data in row.Data.Where(r => !r.Column.IdColumn))
            {
                sql.Append($"{data.Column.ColumnName} = {_dataToValueConverter.Convert(data)}");

                if (data != row.Data.Last())
                    sql.AppendLine(",");
                else
                    sql.AppendLine();
            }
            sql.AppendLine("WHERE");
            var idQuery = row.Data.Where(r => r.Column.IdColumn).Select(r => $"{r.Column.ColumnName} = {r.Value}");
            sql.Append(string.Join(" AND ", idQuery));
            sql.AppendLine(";");
        }

        private void WriteDelete(StringBuilder sql, DatabaseRow row)
        {
            /**
            DELETE FROM tablename WHERE Id NOT IN (id1, id2)
             */

            var idColumns = row.Data.Where(r => r.Column.IdColumn).ToList();
            var idQuery = string.Join(" AND ", idColumns.Select(r => $"t.{r.Column.ColumnName} = {_dataToValueConverter.Convert(r)}"));
            sql.Append($"DELETE FROM {_tableConfiguration.TableName} WHERE NOT EXISTS (SELECT 1 FROM {_tableConfiguration.TableName} AS t WHERE {idQuery})");
            sql.AppendLine(";");
        }

        private string GenerateValuesForRow(DatabaseRow row)
        {
            var commaSeparatedValues = string.Join(",", row.Data.Select(r => _dataToValueConverter.Convert(r)));
            return $"({commaSeparatedValues})";
        }
    }
}
