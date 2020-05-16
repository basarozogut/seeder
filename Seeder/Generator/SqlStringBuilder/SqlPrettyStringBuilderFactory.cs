using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seeder.Generator.SqlStringBuilder
{
    class SqlPrettyStringBuilderFactory : ISqlStringBuilderFactory
    {
        public ISqlStringBuilder CreateSqlStringBuilder()
        {
            return new SqlPrettyStringBuilder();
        }
    }
}
