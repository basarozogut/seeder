# seeder
Generate SQL seed script for your enumeration tables.

## Security Warning
If your seed data source (aka enumeration tables) contains any unescaped SQL strings, your generated SQL script may contain SQL injection. It's your responsibility to validate and escape your enumeration table data for possible SQL injection attacks.

## Supported Databases
seeder currently supports code generation for MSSQL and MySQL databases.

## Sample Usage

The following code will generate seed code for *single_key_example* and *composite_key_example*. You can then save the generated sql string to a file and run it directly on a CLI, in a sql management environment or add it to your deployment script etc.

```c#
var dbConfig = new DatabaseConfiguration()
{
    Tables = new List<TableConfiguration>() {
        TableConfiguration.CreateDefault("single_key_example"),
        TableConfiguration.CreateDefault("composite_key_example")
    }
};

 // Convenient method. Usually enough for most cases.
using (var connection = new MySqlConnection("your_connection_string_here"))
{
    connection.Open();
    var sql = MysqlGenerator.GenerateSql(dbConfig, connection);
}

// Detailed method. When you want to use custom data access layers, create the generator with a factory or use dependency injection for creating the generator etc.
using (var connection = new MySqlConnection("your_connection_string_here"))
{
    connection.Open();
    var generator = new MysqlGenerator(dbConfig, new MysqlDataAccess(connection));
    var sql = generator.GenerateSql();
}
```

## Generator Architecture
The code generators are split into different assemblies for dependency management, isolation and testability. Different assemblies means you only need to reference *Seeder* and *Seeder.Mssql* assemblies if you only want to use Mssql code generation feature. The projects are:
* *Seeder* project contains core classes and interfaces which all generators use.
* *Seeder.Mysql* projects contains the Mysql spesific implementation.
* *Seeder.Mssql* project contains the Mssql spesific implementation.
* *Seeder.UnitTests* projects contains the unit tests for all of the above.

The pluggable architecture means you can use your favorite depedency injection library with the interfaces (e.g. ISqlGenerator, IDataAccess).
