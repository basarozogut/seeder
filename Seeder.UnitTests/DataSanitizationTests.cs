using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Seeder.Generator.DataObjects;
using Seeder.Generator.Mssql;
using Seeder.Generator.Mysql;

namespace Seeder.UnitTests
{
    [TestClass]
    public class DataSanitizationTests
    {
        [TestMethod]
        public void TestMssqlStringSanitization()
        {
            var converter = new MssqlDataToValueConverter();

            var nvarcharColumn = new DatabaseColumn()
            {
                DataType = "nvarchar"
            };

            Assert.AreEqual(converter.Convert(new DatabaseData() { Value = "test", Column = nvarcharColumn }), "'test'");
            Assert.AreEqual(converter.Convert(new DatabaseData() { Value = "te'st", Column = nvarcharColumn }), "'te''st'");
            Assert.AreEqual(converter.Convert(new DatabaseData() { Value = "te's't", Column = nvarcharColumn }), "'te''s''t'");
        }

        [TestMethod]
        public void TestMysqlStringSanitization()
        {
            var converter = new MysqlDataToValueConverter();

            var nvarcharColumn = new DatabaseColumn()
            {
                DataType = "nvarchar"
            };

            Assert.AreEqual(converter.Convert(new DatabaseData() { Value = "test", Column = nvarcharColumn }), "'test'");
            Assert.AreEqual(converter.Convert(new DatabaseData() { Value = "te'st", Column = nvarcharColumn }), @"'te\'st'");
            Assert.AreEqual(converter.Convert(new DatabaseData() { Value = "te's't", Column = nvarcharColumn }), @"'te\'s\'t'");
        }
    }
}
