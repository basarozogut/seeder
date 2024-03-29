﻿using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Seeder.Configuration;
using Seeder.Generator.Mssql;
using Seeder.Generator.Interfaces;
using Seeder.Generator.DataObjects;
using Seeder.Generator.Mysql;

namespace Seeder.UnitTests
{
    [TestClass]
    public class SqlGeneratorTests
    {
        class TestObject
        {
            public DatabaseConfiguration DatabaseConfiguration;
            public Mock<IDataAccess> MockDataAccess;
        }

        private TestObject CreateTestObject(bool createCompositeId = false)
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

            DatabaseColumn otherIdColumn = null;
            if (createCompositeId)
            {
                otherIdColumn = new DatabaseColumn() { ColumnName = "OtherId", DataType = "int", IdColumn = true };
            }

            List<DatabaseColumn> columnStructure;
            if (!createCompositeId)
            {
                columnStructure = new List<DatabaseColumn>() { idColumn, usernameColumn, passwordColumn };
            }
            else
            {
                columnStructure = new List<DatabaseColumn>() { idColumn, otherIdColumn, usernameColumn, passwordColumn };
            }

            var rows = new List<DatabaseRow>()
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

            if (createCompositeId)
            {
                rows[0].Data.Insert(1, new DatabaseData() { Column = otherIdColumn, Value = 44 });
                rows[1].Data.Insert(1, new DatabaseData() { Column = otherIdColumn, Value = 45 });
            }

            var mockDataAccess = new Mock<IDataAccess>();
            mockDataAccess.Setup(da => da.GetColumnStructureFromDatabase(databaseConfiguration.Tables[0])).Returns(columnStructure);
            mockDataAccess.Setup(da => da.GetDataForTable(databaseConfiguration.Tables[0], columnStructure)).Returns(rows);

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

            var generator = new MssqlGenerator(testObject.DatabaseConfiguration, testObject.MockDataAccess.Object);
            var generatedSql = generator.GenerateSql();
            const string expectedSql =
@"-- Seed for [Users]
MERGE Users AS t USING (VALUES
(1,'User 1','1234'),
(2,'User 2','5678')
) AS s (Id, Username, Password) ON (s.Id = t.Id)
WHEN MATCHED
THEN UPDATE SET
t.Username = s.Username,
t.Password = s.Password
WHEN NOT MATCHED BY TARGET
THEN INSERT (Id, Username, Password)
VALUES (s.Id, s.Username, s.Password)
WHEN NOT MATCHED BY SOURCE
THEN DELETE
;";
            Assert.AreEqual(generatedSql, expectedSql);
        }

        [TestMethod]
        public void TestMssqlMergeGenerationForCompositeId()
        {
            var testObject = CreateTestObject(true);

            var generator = new MssqlGenerator(testObject.DatabaseConfiguration, testObject.MockDataAccess.Object);
            var generatedSql = generator.GenerateSql();
            const string expectedSql =
@"-- Seed for [Users]
MERGE Users AS t USING (VALUES
(1,44,'User 1','1234'),
(2,45,'User 2','5678')
) AS s (Id, OtherId, Username, Password) ON (s.Id = t.Id) AND (s.OtherId = t.OtherId)
WHEN MATCHED
THEN UPDATE SET
t.Username = s.Username,
t.Password = s.Password
WHEN NOT MATCHED BY TARGET
THEN INSERT (Id, OtherId, Username, Password)
VALUES (s.Id, s.OtherId, s.Username, s.Password)
WHEN NOT MATCHED BY SOURCE
THEN DELETE
;";
            Assert.AreEqual(generatedSql, expectedSql);
        }

        [TestMethod]
        public void TestMysqlInsertOrUpdateGeneration()
        {
            var testObject = CreateTestObject();
            testObject.DatabaseConfiguration.Tables[0].EnableInsert = true;
            testObject.DatabaseConfiguration.Tables[0].EnableUpdate = true;
            testObject.DatabaseConfiguration.Tables[0].EnableDelete = false;

            var generator = new MysqlGenerator(testObject.DatabaseConfiguration, testObject.MockDataAccess.Object);
            var generatedSql = generator.GenerateSql();
            const string expectedSql =
@"-- Seed for [Users]
INSERT INTO Users
(Id, Username, Password)
VALUES
(1,'User 1','1234')
ON DUPLICATE KEY UPDATE
Username = 'User 1',
Password = '1234';
INSERT INTO Users
(Id, Username, Password)
VALUES
(2,'User 2','5678')
ON DUPLICATE KEY UPDATE
Username = 'User 2',
Password = '5678';";
            Assert.AreEqual(generatedSql, expectedSql);
        }

        [TestMethod]
        public void TestMysqlInsertOrUpdateGenerationForCompositeId()
        {
            var testObject = CreateTestObject(true);
            testObject.DatabaseConfiguration.Tables[0].EnableInsert = true;
            testObject.DatabaseConfiguration.Tables[0].EnableUpdate = true;
            testObject.DatabaseConfiguration.Tables[0].EnableDelete = false;

            var generator = new MysqlGenerator(testObject.DatabaseConfiguration, testObject.MockDataAccess.Object);
            var generatedSql = generator.GenerateSql();
            const string expectedSql =
@"-- Seed for [Users]
INSERT INTO Users
(Id, OtherId, Username, Password)
VALUES
(1,44,'User 1','1234')
ON DUPLICATE KEY UPDATE
Username = 'User 1',
Password = '1234';
INSERT INTO Users
(Id, OtherId, Username, Password)
VALUES
(2,45,'User 2','5678')
ON DUPLICATE KEY UPDATE
Username = 'User 2',
Password = '5678';";
            Assert.AreEqual(generatedSql, expectedSql);
        }

        [TestMethod]
        public void TestMysqlInsertOnlyGeneration()
        {
            var testObject = CreateTestObject();
            testObject.DatabaseConfiguration.Tables[0].EnableInsert = true;
            testObject.DatabaseConfiguration.Tables[0].EnableUpdate = false;
            testObject.DatabaseConfiguration.Tables[0].EnableDelete = false;

            var generator = new MysqlGenerator(testObject.DatabaseConfiguration, testObject.MockDataAccess.Object);
            var generatedSql = generator.GenerateSql();
            const string expectedSql =
@"-- Seed for [Users]
INSERT INTO Users
(Id, Username, Password)
VALUES
(1,'User 1','1234'),
(2,'User 2','5678');";
            Assert.AreEqual(generatedSql, expectedSql);
        }

        [TestMethod]
        public void TestMysqlInsertOnlyGenerationForCompositeId()
        {
            var testObject = CreateTestObject(true);
            testObject.DatabaseConfiguration.Tables[0].EnableInsert = true;
            testObject.DatabaseConfiguration.Tables[0].EnableUpdate = false;
            testObject.DatabaseConfiguration.Tables[0].EnableDelete = false;

            var generator = new MysqlGenerator(testObject.DatabaseConfiguration, testObject.MockDataAccess.Object);
            var generatedSql = generator.GenerateSql();
            const string expectedSql =
@"-- Seed for [Users]
INSERT INTO Users
(Id, OtherId, Username, Password)
VALUES
(1,44,'User 1','1234'),
(2,45,'User 2','5678');";
            Assert.AreEqual(generatedSql, expectedSql);
        }

        [TestMethod]
        public void TestMysqlUpdateOnlyGeneration()
        {
            var testObject = CreateTestObject();
            testObject.DatabaseConfiguration.Tables[0].EnableInsert = false;
            testObject.DatabaseConfiguration.Tables[0].EnableUpdate = true;
            testObject.DatabaseConfiguration.Tables[0].EnableDelete = false;

            var generator = new MysqlGenerator(testObject.DatabaseConfiguration, testObject.MockDataAccess.Object);
            var generatedSql = generator.GenerateSql();
            const string expectedSql =
@"-- Seed for [Users]
UPDATE Users
SET
Username = 'User 1',
Password = '1234'
WHERE
Id = 1;
UPDATE Users
SET
Username = 'User 2',
Password = '5678'
WHERE
Id = 2;";
            Assert.AreEqual(generatedSql, expectedSql);
        }

        [TestMethod]
        public void TestMysqlUpdateOnlyGenerationForCompositeId()
        {
            var testObject = CreateTestObject(true);
            testObject.DatabaseConfiguration.Tables[0].EnableInsert = false;
            testObject.DatabaseConfiguration.Tables[0].EnableUpdate = true;
            testObject.DatabaseConfiguration.Tables[0].EnableDelete = false;

            var generator = new MysqlGenerator(testObject.DatabaseConfiguration, testObject.MockDataAccess.Object);
            var generatedSql = generator.GenerateSql();
            const string expectedSql =
@"-- Seed for [Users]
UPDATE Users
SET
Username = 'User 1',
Password = '1234'
WHERE
Id = 1 AND OtherId = 44;
UPDATE Users
SET
Username = 'User 2',
Password = '5678'
WHERE
Id = 2 AND OtherId = 45;";
            Assert.AreEqual(generatedSql, expectedSql);
        }

        [TestMethod]
        public void TestMysqlDeleteOnlyGeneration()
        {
            var testObject = CreateTestObject();
            testObject.DatabaseConfiguration.Tables[0].EnableInsert = false;
            testObject.DatabaseConfiguration.Tables[0].EnableUpdate = false;
            testObject.DatabaseConfiguration.Tables[0].EnableDelete = true;

            var generator = new MysqlGenerator(testObject.DatabaseConfiguration, testObject.MockDataAccess.Object);
            var generatedSql = generator.GenerateSql();
            const string expectedSql =
@"-- Seed for [Users]
DELETE FROM Users WHERE ((Id <> 1) AND (Id <> 2));";
            Assert.AreEqual(generatedSql, expectedSql);
        }

        [TestMethod]
        public void TestMysqlDeleteOnlyGenerationForCompositeId()
        {
            var testObject = CreateTestObject(true);
            testObject.DatabaseConfiguration.Tables[0].EnableInsert = false;
            testObject.DatabaseConfiguration.Tables[0].EnableUpdate = false;
            testObject.DatabaseConfiguration.Tables[0].EnableDelete = true;

            var generator = new MysqlGenerator(testObject.DatabaseConfiguration, testObject.MockDataAccess.Object);
            var generatedSql = generator.GenerateSql();
            const string expectedSql =
@"-- Seed for [Users]
DELETE FROM Users WHERE ((Id <> 1 OR OtherId <> 44) AND (Id <> 2 OR OtherId <> 45));";
            Assert.AreEqual(generatedSql, expectedSql);
        }
    }
}
