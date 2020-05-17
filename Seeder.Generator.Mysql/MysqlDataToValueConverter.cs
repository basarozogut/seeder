using Seeder.Generator.DataObjects;
using Seeder.Generator.Interfaces;

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
                return $"'{EscapePossibleInjection((string)data.Value)}'";

            if (data.Column.DataType == "int")
                return ((int)data.Value).ToString();

            throw new SqlGeneratorException($"Unsupported data type: {data.Column.DataType}");
        }

        private string EscapePossibleInjection(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return value.Replace("'", @"\'");
            }

            return value;
        }
    }
}
