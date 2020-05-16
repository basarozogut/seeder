using Seeder.Generator.Interfaces;

namespace Seeder.Generator.Mssql.SqlStringBuilder
{
    class SqlCompactStringBuilderFactory : ISqlStringBuilderFactory
    {
        public ISqlStringBuilder CreateSqlStringBuilder()
        {
            return new SqlCompactStringBuilder();
        }
    }
}
