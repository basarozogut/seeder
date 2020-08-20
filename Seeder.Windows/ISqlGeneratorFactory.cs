using Seeder.Configuration;
using Seeder.Generator.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seeder.Windows
{
    interface ISqlGeneratorFactory
    {
        ISqlGenerator CreateSqlGenerator(DatabaseConfiguration databaseConfiguration, string databaseType, string connectionString, out DbConnection dbConnection);
    }
}
