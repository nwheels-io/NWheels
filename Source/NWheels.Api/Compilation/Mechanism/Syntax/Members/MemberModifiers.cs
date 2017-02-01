using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Members
{
    [Flags]
    public enum MemberModifiers
    {
        None = 0,
        Static = 0x01,
        Abstract = 0x02,
        Virtual = 0x04,
        Override = 0x08,
        Async = 0x10
    }
}
