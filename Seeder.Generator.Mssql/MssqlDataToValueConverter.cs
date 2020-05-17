using Seeder.Generator.DataObjects;
using Seeder.Generator.Interfaces;

namespace Seeder.Generator.Mssql
{
    public class MssqlDataToValueConverter : IDataToValueConverter
    {
        public string Convert(DatabaseData data)
        {
            if (data.Value == null)
                return "NULL";

            if (data.Column.DataType == "nvarchar")
                return $"'{EscapePossibleInjection((string)data.Value)}'";

            if (data.Column.DataType == "bit")
                return ((bool)data.Value) ? "1" : "0";

            if (data.Column.DataType == "int")
                return ((int)data.Value).ToString();

            if (data.Column.DataType == "datetime") // conversion not supported for now
                return "NULL"; // TODO make conversion

            throw new SqlGeneratorException($"Unsupported data type: {data.Column.DataType}");
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
