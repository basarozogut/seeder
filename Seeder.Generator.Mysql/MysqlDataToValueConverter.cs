using Seeder.Generator.DataObjects;
using Seeder.Generator.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seeder.Generator.Mysql
{
    public class MysqlDataToValueConverter : IDataToValueConverter
    {
        public string Convert(DatabaseData data)
        {
            if (data.Value == null)
                return "NULL";

            if (data.Column.DataType.StartsWith("varchar") ||
                data.Column.DataType.StartsWith("nvarchar"))
                return $"'{data.Value}'";

            return EscapePossibleInjection(data.Value.ToString());
        }

        private string EscapePossibleInjection(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return value.Replace("'", "\'");
            }

            return value;
        }
    }
}
