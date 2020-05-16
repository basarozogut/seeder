using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Seeder.Configuration;
using Seeder.Generator.Mssql;
using Seeder.Generator.Interfaces;
using Seeder.Generator.DataObjects;

namespace Seeder.UnitTests
{
    [TestClass]
    public class MssqlGeneratorTests
    {
        [TestMethod]
        public void TestAutoGeneration()
        {
            var databaseConfiguration = new DatabaseConfiguration()
            {
                DatabaseType = DatabaseType.Mssql,
                ConnectionString = "just testing",
                Tables = new List<TableConfiguration>()
                {
                    TableConfiguration.CreateDefault("Users")
                }
            };

            var idColumn = new DatabaseColumn() { ColumnName = "Id", DataType = "int" };
            var usernameColumn = new DatabaseColumn() { ColumnName = "Username", DataType = "nvarchar" };
            var passwordColumn = new DatabaseColumn() { ColumnName = "Password", DataType = "nvarchar" };

            var columnStructure = new List<DatabaseColumn>() { idColumn, usernameColumn, passwordColumn };

            var data = new List<DatabaseRow>()
            {
                new DatabaseRow()
                {
                    Data = new List<DatabaseData>()
                    {
                        new DatabaseData() { Column = idColumn, Value = 1 },
                        new DatabaseData() { Column = usernameColumn, Value = "User 1" },
                        new DatabaseData() { Column = passwordColumn, Value = "1234" },
                    }
                },
                new DatabaseRow()
                {
                    Data = new List<DatabaseData>()
                    {
                        new DatabaseData() { Column = idColumn, Value = 2 },
                        new DatabaseData() { Column = usernameColumn, Value = "User 2" },
                        new DatabaseData() { Column = passwordColumn, Value = "5678" },
                    }
                }
            };

            var mockDataAccess = new Mock<IDataAccess>();
            mockDataAccess.Setup(da => da.GetColumnStructureFromDatabase(databaseConfiguration.Tables[0])).Returns(columnStructure);
            mockDataAccess.Setup(da => da.GetDataForTable(databaseConfiguration.Tables[0], columnStructure)).Returns(data);

            var generator = new MssqlGenerator(databaseConfiguration, mockDataAccess.Object, false);
            var generatedSql = generator.GenerateSql();
            const string expectedSql =
                "MERGE Users AS t USING (VALUES (1,'User 1','1234'), (2,'User 2','5678') ) AS s (Id, Username, Password) ON (s.Id = t.Id) WHEN MATCHED THEN UPDATE SET t.Username = s.Username, t.Password = s.Password WHEN NOT MATCHED BY TARGET THEN INSERT(Id, Username, Password) VALUES(s.Id, s.Username, s.Password) WHEN NOT MATCHED BY SOURCE THEN DELETE;";
            Assert.AreEqual(generatedSql, expectedSql);
        }
    }
}
