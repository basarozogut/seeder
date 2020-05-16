using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seeder.Generator
{
    public class SqlGeneratorException : Exception
    {
        public SqlGeneratorException(string message) : base(message)
        {
        }
    }
}
