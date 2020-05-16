using Seeder.Generator.Interfaces;

namespace Seeder.Generator.Mssql.SqlStringBuilder
{
    class SqlPrettyStringBuilderFactory : ISqlStringBuilderFactory
    {
        public ISqlStringBuilder CreateSqlStringBuilder()
        {
            return new SqlPrettyStringBuilder();
        }
    }
}
