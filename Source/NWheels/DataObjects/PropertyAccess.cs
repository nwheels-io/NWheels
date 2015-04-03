using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.DataObjects
{
    [Flags]
    public enum PropertyAccess
    {
        None = 0,
        Read = 0x01,
        Write = 0x02,
        Search = 0x04,
        ReadOnly = Read | Search,
        WriteOnly = Write,
        ReadWrite = Read | Write,
        SearchOnly = Search
    }
}
