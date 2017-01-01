using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Api.Compilation.Syntax.Members
{
    [Flags]
    public enum MemberModifiers
    {
        None = 0,
        Abstract = 0x01,
        Static = 0x02,
        Override = 0x04,
        Async = 0x08
    }
}
