using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seeder.Configuration
{
    public class DatabaseConfiguration
    {
        public DatabaseType DatabaseType { get; set; }
        public string ConnectionString { get; set; }
        public List<TableConfiguration> Tables { get; set; }
    }

    public enum DatabaseType
    {
        Mssql,
        Mysql
    }
}
