using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seeder.Generator.SqlStringBuilder
{
    class SqlCompactStringBuilder : ISqlStringBuilder
    {
        private readonly StringBuilder _stringBuilder;

        public SqlCompactStringBuilder()
        {
            _stringBuilder = new StringBuilder();
        }

        public void Append(string value)
        {
            _stringBuilder.Append(value);
        }

        public void AppendLine(string value)
        {
            _stringBuilder.Append($"{value} ");
        }

        public void AppendCommentLine(string value)
        {
            // Don't need comments on compact sql strings.
        }

        public void EndStatement()
        {
            _stringBuilder.Append(";");
        }

        public void AppendLine()
        {
            _stringBuilder.Append(" ");
        }

        public override string ToString()
        {
            return _stringBuilder.ToString();
        }
    }
}
