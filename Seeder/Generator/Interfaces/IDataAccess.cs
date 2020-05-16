using System;
using System.Collections.Generic;
using Seeder.Configuration;
using Seeder.Generator.DataObjects;

namespace Seeder.Generator.Interfaces
{
    interface IDataAccess : IDisposable
    {
        List<DatabaseColumn> GetColumnStructureFromDatabase(TableConfiguration tableConfiguration);
        List<List<DatabaseData>> GetDataForTable(TableConfiguration tableConfiguration, List<DatabaseColumn> databaseColumns);
    }
}