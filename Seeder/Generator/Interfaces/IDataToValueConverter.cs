using Seeder.Generator.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seeder.Generator.Interfaces
{
    public interface IDataToValueConverter
    {
        string Convert(DatabaseData data);
    }
}
