using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Core.Logging
{
    internal interface IThreadLogAnchor
    {
        ThreadLog CurrentThreadLog { get; set; }
    }
}
