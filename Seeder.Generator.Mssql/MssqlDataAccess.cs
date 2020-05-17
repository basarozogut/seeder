using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Seeder.Configuration;
using Seeder.Generator.DataObjects;
using Seeder.Generator.Interfaces;

namespace Seeder.Generator.Mssql
{
    sealed class MssqlDataAccess : IDataAccess
    {
        private readonly SqlConnection _connection;

        public MssqlDataAccess(SqlConnection connection)
        {
            _connection = connection;
        }

        public List<DatabaseColumn> GetColumnStructureFromDatabase(TableConfiguration tableConfiguration)
        {
            const string sql =
                     @"SELECT 
                       SCHEMA_NAME(T.schema_id) AS SchemaName,
	                   T.name AS TableName,
                       C.name AS ColumnName,
                       P.name AS DataType,
                       P.max_length AS Size 
		                FROM   sys.objects AS T
			                   JOIN sys.columns AS C ON T.object_id = C.object_id
			                   JOIN sys.types AS P ON C.system_type_id = P.system_type_id
		                WHERE  T.type_desc = 'USER_TABLE' AND P.name <> 'sysname' AND T.name = @tableName AND SCHEMA_NAME(T.schema_id) = @schemaName";
            var cmd = new SqlCommand(sql, _connection);
            cmd.Parameters.AddWithValue("@schemaName", tableConfiguration.SchemaName);
            cmd.Parameters.AddWithValue("@tableName", tableConfiguration.TableName);
            var da = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            da.Fill(dt);

            return dt.Rows.Cast<DataRow>().Select(r => new DatabaseColumn()
            {
                ColumnName = (string)r["ColumnName"],
                DataType = (string)r["DataType"]
            }).ToList();
        }

        public List<DatabaseRow> GetDataForTable(TableConfiguration tableConfiguration, List<DatabaseColumn> databaseColumns)
        {
            MakeSecurityCheckForTable(tableConfiguration, databaseColumns);

            var sql =
                $"SELECT {string.Join(",", tableConfiguration.Columns)} FROM {tableConfiguration.TableName}";
            var cmd = new SqlCommand(sql, _connection);
            var da = new SqlDataAdapter(cmd);
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

        private void MakeSecurityCheckForTable(TableConfiguration tableConfiguration, List<DatabaseColumn> databaseColumns)
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
