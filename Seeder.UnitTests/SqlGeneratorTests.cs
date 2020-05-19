using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Seeder.Configuration;
using Seeder.Generator.Mssql;
using Seeder.Generator.Interfaces;
using Seeder.Generator.DataObjects;
using Seeder.Generator.SqlStringBuilder;
using Seeder.Generator.Mysql;

namespace Seeder.UnitTests
{
    [TestClass]
    public class SqlGeneratorTests
    {
        class TestObject {
            public DatabaseConfiguration DatabaseConfiguration;
            public Mock<IDataAccess> MockDataAccess;
        }

        private TestObject CreateTestObject()
        {
            var databaseConfiguration = new DatabaseConfiguration()
            {
                Tables = new List<TableConfiguration>()
                {
                    TableConfiguration.CreateDefault("Users")
                }
            };

            var idColumn = new DatabaseColumn() { ColumnName = "Id", DataType = "int", IdColumn = true };
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

            return new TestObject
            {
                DatabaseConfiguration = databaseConfiguration,
                MockDataAccess = mockDataAccess
            };
        }

        [TestMethod]
        public void TestMssqlMergeGeneration()
        {
            var testObject = CreateTestObject();

            var generator = new MssqlGenerator(testObject.DatabaseConfiguration, testObject.MockDataAccess.Object, new SqlCompactStringBuilderFactory());
            var generatedSql = generator.GenerateSql();
            const string expectedSql =
                "MERGE Users AS t USING (VALUES (1,'User 1','1234'), (2,'User 2','5678')) AS s (Id, Username, Password) ON (s.Id = t.Id) WHEN MATCHED THEN UPDATE SET t.Username = s.Username, t.Password = s.Password WHEN NOT MATCHED BY TARGET THEN INSERT(Id, Username, Password) VALUES(s.Id, s.Username, s.Password) WHEN NOT MATCHED BY SOURCE THEN DELETE;";
            Assert.AreEqual(generatedSql, expectedSql);
        }

        [TestMethod]
        public void TestMysqlInsertOrUpdateGeneration()
        {
            var testObject = CreateTestObject();
            testObject.DatabaseConfiguration.Tables[0].EnableInsert = true;
            testObject.DatabaseConfiguration.Tables[0].EnableUpdate = true;
            testObject.DatabaseConfiguration.Tables[0].EnableDelete = false;

            var generator = new MysqlGenerator(testObject.DatabaseConfiguration, testObject.MockDataAccess.Object, new SqlCompactStringBuilderFactory());
            var generatedSql = generator.GenerateSql();
            const string expectedSql =
                "INSERT INTO Users (Id, Username, Password) VALUES (1,'User 1','1234') ON DUPLICATE KEY UPDATE Username = 'User 1', Password = '1234';INSERT INTO Users (Id, Username, Password) VALUES (2,'User 2','5678') ON DUPLICATE KEY UPDATE Username = 'User 2', Password = '5678';";
            Assert.AreEqual(generatedSql, expectedSql);            
        }

        [TestMethod]
        public void TestMysqlInsertOnlyGeneration()
        {
            var testObject = CreateTestObject();
            testObject.DatabaseConfiguration.Tables[0].EnableInsert = true;
            testObject.DatabaseConfiguration.Tables[0].EnableUpdate = false;
            testObject.DatabaseConfiguration.Tables[0].EnableDelete = false;

            var generator = new MysqlGenerator(testObject.DatabaseConfiguration, testObject.MockDataAccess.Object, new SqlCompactStringBuilderFactory());
            var generatedSql = generator.GenerateSql();
            const string expectedSql =
                "INSERT INTO Users (Id, Username, Password) VALUES (1,'User 1','1234'), (2,'User 2','5678');";
            Assert.AreEqual(generatedSql, expectedSql);
        }

        [TestMethod]
        public void TestMysqlUpdateOnlyGeneration()
        {
            var testObject = CreateTestObject();
            testObject.DatabaseConfiguration.Tables[0].EnableInsert = false;
            testObject.DatabaseConfiguration.Tables[0].EnableUpdate = true;
            testObject.DatabaseConfiguration.Tables[0].EnableDelete = false;

            var generator = new MysqlGenerator(testObject.DatabaseConfiguration, testObject.MockDataAccess.Object, new SqlCompactStringBuilderFactory());
            var generatedSql = generator.GenerateSql();
            const string expectedSql =
                "UPDATE Users SET Username = 'User 1', Password = '1234' WHERE Id = 1;UPDATE Users SET Username = 'User 2', Password = '5678' WHERE Id = 2;";
            Assert.AreEqual(generatedSql, expectedSql);
        }

        [TestMethod]
        public void TestMysqlDeleteOnlyGeneration()
        {
            var testObject = CreateTestObject();
            testObject.DatabaseConfiguration.Tables[0].EnableInsert = false;
            testObject.DatabaseConfiguration.Tables[0].EnableUpdate = false;
            testObject.DatabaseConfiguration.Tables[0].EnableDelete = true;

            var generator = new MysqlGenerator(testObject.DatabaseConfiguration, testObject.MockDataAccess.Object, new SqlCompactStringBuilderFactory());
            var generatedSql = generator.GenerateSql();
            const string expectedSql =
                "DELETE FROM Users WHERE NOT EXISTS (SELECT 1 FROM Users AS t WHERE t.Id = 1);DELETE FROM Users WHERE NOT EXISTS (SELECT 1 FROM Users AS t WHERE t.Id = 2);";
            Assert.AreEqual(generatedSql, expectedSql);
        }
    }
}
