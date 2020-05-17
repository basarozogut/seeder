using Seeder.Generator.Interfaces;

namespace Seeder.Generator.SqlStringBuilder
{
    public class SqlPrettyStringBuilderFactory : ISqlStringBuilderFactory
    {
        public ISqlStringBuilder CreateSqlStringBuilder()
        {
            return new SqlPrettyStringBuilder();
        }
    }
}
