using Seeder.Generator.Interfaces;
using System.Text;

namespace Seeder.Generator.Mssql.SqlStringBuilder
{
    class SqlPrettyStringBuilder : ISqlStringBuilder
    {
        private readonly StringBuilder _stringBuilder;

        public SqlPrettyStringBuilder()
        {
            _stringBuilder = new StringBuilder();
        }

        public void Append(string value)
        {
            _stringBuilder.Append(value);
        }

        public void AppendLine(string value)
        {
            _stringBuilder.AppendLine(value);
        }

        public void AppendCommentLine(string value)
        {
            _stringBuilder.AppendLine(value);
        }

        public void EndStatement()
        {
            _stringBuilder.AppendLine(";");
        }

        public void AppendLine()
        {
            _stringBuilder.AppendLine();
        }

        public override string ToString()
        {
            return _stringBuilder.ToString();
        }
    }
}
