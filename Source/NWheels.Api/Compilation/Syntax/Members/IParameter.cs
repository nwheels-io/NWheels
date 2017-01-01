using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Api.Compilation.Syntax.Members
{
    public interface IParameter
    {
        string Name { get; }
        int? Position { get; }
        ITypeMember Type { get; }
        ParameterModifier Modifier { get; }
    }
}
