using Seeder.Generator.Interfaces;

namespace Seeder.Generator.SqlStringBuilder
{
    public class SqlCompactStringBuilderFactory : ISqlStringBuilderFactory
    {
        public ISqlStringBuilder CreateSqlStringBuilder()
        {
            return new SqlCompactStringBuilder();
        }
    }
}
