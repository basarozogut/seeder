using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Seeder.Configuration;

namespace Seeder.Windows
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length <= 0)
            {
                PrintUsage();
            }
            else if (args.Length == 4)
            {
                var databaseType = args[0];
                var connectionString = args[1];
                var configPath = args[2];
                var outputFilePath = args[3];

                GenerateSql(databaseType, connectionString, configPath, outputFilePath);
            }
            else
            {
                PrintUsage();
            }
        }

        static void PrintUsage()
        {
            Console.WriteLine("seeder usage:");
            Console.WriteLine("seeder.exe [database-type] [connection-string] [config-file-path] [output-file-path]");
            Console.WriteLine();
            Console.WriteLine("Possible [database-type] [connection-string] values:");
            Console.WriteLine("mssql, mysql");
            Console.WriteLine();
            Console.WriteLine("seeder usage example:");
            Console.WriteLine("seeder.exe mysql \"Server=localhost;Database=seederexample;Uid=user;Pwd=pass;\" C:\\config.json C:\\output.sql");
        }

        static void GenerateSql(string databaseType, string connectionString, string configPath, string outputFilePath)
        {
            var factory = CreateSqlGeneratorFactory();

            var cfgString = File.ReadAllText(configPath);
            var cfg = JsonConvert.DeserializeObject<DatabaseConfiguration>(cfgString);

            DbConnection connection;
            var generator = factory.CreateSqlGenerator(cfg, databaseType, connectionString, out connection);
            using (connection)
            {
                connection.Open();

                var sql = generator.GenerateSql();
                File.WriteAllText(outputFilePath, sql);
            }
        }

        static ISqlGeneratorFactory CreateSqlGeneratorFactory()
        {
            return new SqlGeneratorFactory();
        }
    }
}
