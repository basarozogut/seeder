using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Seeder.Configuration;
using Seeder.Generator.DataObjects;
using Seeder.Generator.Interfaces;
using MySql.Data.MySqlClient;

namespace Seeder.Generator.Mysql
{
    sealed class MysqlDataAccess : IDataAccess
    {
        private readonly MySqlConnection _connection;

        public MysqlDataAccess(MySqlConnection connection)
        {
            _connection = connection;
        }

        public List<DatabaseColumn> GetColumnStructureFromDatabase(TableConfiguration tableConfiguration)
        {
            CheckTableNameForSecurity(tableConfiguration);

            var sql =
                     $"SHOW COLUMNS FROM {tableConfiguration.TableName}";
            using (var cmd = new MySqlCommand(sql, _connection))
            {
                cmd.Parameters.AddWithValue("@schemaName", tableConfiguration.SchemaName);
                cmd.Parameters.AddWithValue("@tableName", tableConfiguration.TableName);
                var da = new MySqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);

                return dt.Rows.Cast<DataRow>().Select(r => new DatabaseColumn()
                {
                    IdColumn = tableConfiguration.IdColumns.Contains((string)r["Field"]),
                    ColumnName = (string)r["Field"],
                    DataType = (string)r["Type"],
                }).ToList();
            }
        }

        public List<DatabaseRow> GetDataForTable(TableConfiguration tableConfiguration, List<DatabaseColumn> databaseColumns)
        {
            CheckTableNameForSecurity(tableConfiguration);
            MakeSecurityCheckForTableColumns(tableConfiguration, databaseColumns);

            var sql =
                $"SELECT {string.Join(",", tableConfiguration.Columns)} FROM {tableConfiguration.TableName}";
            var cmd = new MySqlCommand(sql, _connection);
            var da = new MySqlDataAdapter(cmd);
            var dt = new DataTable();
            da.Fill(dt);

            var tempRows = new List<DatabaseRow>();

            foreach (DataRow row in dt.Rows)
            {
                var tempRow = new List<DatabaseData>();
                foreach (var columnName in tableConfiguration.Columns)
                {
                    var val = row[columnName];
                    var tempVal = new DatabaseData();
                    if (val == null || val == DBNull.Value)
                    {
                        tempVal.Value = null;
                    }
                    else
                    {
                        tempVal.Value = val;
                    }
                    tempVal.Column = databaseColumns.Single(r => r.ColumnName == columnName);
                    tempRow.Add(tempVal);
                }
                tempRows.Add(new DatabaseRow()
                {
                    Data = tempRow
                });
            }

            return tempRows;
        }

        private void CheckTableNameForSecurity(TableConfiguration tableConfiguration)
        {
            const string sql = "SHOW TABLES";
            using (var cmd = new MySqlCommand(sql, _connection))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var tableName = (string)reader[0];
                        if (tableName == tableConfiguration.TableName)
                        {
                            return;
                        }
                    }
                }
            }

            throw new SqlGeneratorException($"Table name ({tableConfiguration.TableName}) is not present in database!");
        }

        private void MakeSecurityCheckForTableColumns(TableConfiguration tableConfiguration, List<DatabaseColumn> databaseColumns)
        {
            var databaseColumnNames = databaseColumns.Select(r => r.ColumnName).ToList();
            foreach (var column in tableConfiguration.Columns)
            {
                if (!databaseColumnNames.Contains(column))
                    throw new SqlGeneratorException($"Column not found in database! ({column})");
            }
        }
    }
}
