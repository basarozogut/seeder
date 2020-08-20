using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seeder.Windows
{
    class SeederException : Exception
    {
        public SeederException()
        {
        }

        public SeederException(string message) : base(message)
        {
        }
    }
}
