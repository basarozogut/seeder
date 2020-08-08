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
                WriteDelete(sql, rows);
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

        private void WriteDelete(StringBuilder sql, List<DatabaseRow> rows)
        {
            /**
            * Deletes all not-matching rows.
            * 
            * e.g.
            * DELETE FROM tablename WHERE ((Id <> id_value_1 AND OtherId <> other_id_value_1) AND (Id <> id_value_2 AND OtherId <> other_id_value_2))
            */

            var idSubqueries = new List<string>();
            foreach (var row in rows)
            {
                var idColumns = row.Data.Where(r => r.Column.IdColumn).ToList();
                var idSubquery = string.Join(" OR ", idColumns.Select(r => $"{r.Column.ColumnName} <> {_dataToValueConverter.Convert(r)}"));
                idSubqueries.Add(idSubquery);
            }

            var idQuery = string.Join(" AND ", idSubqueries.Select(r => $"({r})"));
            sql.Append($"DELETE FROM {_tableConfiguration.TableName} WHERE ({idQuery})");
            sql.AppendLine(";");
        }

        private string GenerateValuesForRow(DatabaseRow row)
        {
            var commaSeparatedValues = string.Join(",", row.Data.Select(r => _dataToValueConverter.Convert(r)));
            return $"({commaSeparatedValues})";
        }
    }
}
