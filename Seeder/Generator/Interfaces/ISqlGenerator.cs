using System;

namespace Seeder.Generator.Interfaces
{
    public interface ISqlGenerator : IDisposable
    {
        string GenerateSql();
    }
}