using NWheels.Api.Compilation.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Api.Compilation.Syntax.Expressions
{
    public interface INewArrayExpression : IExpression
    {
        ITypeMember ElementType { get; }
        IReadOnlyList<int> DimensionLengths { get; }
    }
}
