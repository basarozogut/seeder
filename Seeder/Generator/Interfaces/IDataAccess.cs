using System;
using System.Collections.Generic;
using Seeder.Configuration;
using Seeder.Generator.DataObjects;

namespace Seeder.Generator.Interfaces
{
    public interface IDataAccess : IDisposable
    {
        List<DatabaseColumn> GetColumnStructureFromDatabase(TableConfiguration tableConfiguration);
        List<DatabaseRow> GetDataForTable(TableConfiguration tableConfiguration, List<DatabaseColumn> databaseColumns);
    }
}