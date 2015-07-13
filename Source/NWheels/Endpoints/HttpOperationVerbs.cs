using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Endpoints
{
    [Flags]
    public enum HttpOperationVerbs
    {
        Get = 1,
        Post = 2,
        Put = 4,
        Delete = 8,
        Head = 16,
        Patch = 32,
        Options = 64,
    }
}
