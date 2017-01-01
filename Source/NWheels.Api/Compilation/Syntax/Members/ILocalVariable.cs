using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Api.Compilation.Syntax.Members
{
    public interface ILocalVariable
    {
        string Name { get; }
        ITypeMember Type { get; }
    }
}
