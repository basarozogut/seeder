using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Seeder.Configuration;
using Seeder.Generator.Interfaces;
using Seeder.Generator.Mssql;

namespace Seeder.Generator
{
    public class SqlGeneratorFactory
    {
        public ISqlGenerator CreateSqlGenerator(DatabaseConfiguration configuration)
        {
            if (configuration.DatabaseType == DatabaseType.Mssql)
            {
                return new MssqlGenerator(configuration, new MssqlDataAccess(configuration.ConnectionString));
            }
            
            throw new SqlGeneratorException("Unsupported database type!");
        }
    }
}
