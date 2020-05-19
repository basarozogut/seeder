using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seeder.Configuration
{
    public class TableConfiguration
    {
        public string SchemaName { get; set; }

        public string TableName { get; set; }

        public List<string> IdColumns { get; set; }

        public List<string> Columns { get; set; }

        public bool AutoFindColumns { get; set; }

        public bool EnableUpdate { get; set; }

        public bool EnableInsert { get; set; } 

        public bool EnableDelete { get; set; }

        public static TableConfiguration CreateDefault(string tableName)
        {
            return new TableConfiguration()
            {
                SchemaName = "dbo",
                TableName = tableName,
                IdColumns = new List<string> { "Id" },
                AutoFindColumns = true,
                EnableUpdate = true,
                EnableInsert = true,
                EnableDelete = true
            };
        }
    }
}
