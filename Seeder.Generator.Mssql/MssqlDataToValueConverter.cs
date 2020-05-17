using Seeder.Generator.DataObjects;
using Seeder.Generator.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seeder.Generator.Mssql
{
    public class MssqlDataToValueConverter : IDataToValueConverter
    {
        public string Convert(DatabaseData data)
        {
            if (data.Value == null)
                return "NULL";

            if (data.Column.DataType == "nvarchar")
                return $"'{data.Value}'";

            if (data.Column.DataType == "bit")
                return ((bool)data.Value) ? "1" : "0";

            if (data.Column.DataType == "datetime") // conversion not supported for now
                return "NULL"; // TODO make conversion

            return EscapePossibleInjection(data.Value.ToString());
        }

        private string EscapePossibleInjection(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return value.Replace("'", "''");
            }

            return value;
        }
    }
}
