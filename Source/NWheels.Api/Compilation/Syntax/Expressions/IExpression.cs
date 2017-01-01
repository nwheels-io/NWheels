using NWheels.Api.Compilation.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Api.Compilation.Syntax.Expressions
{
    public interface IExpression
    {
        ITypeMember Type { get; }
        bool IsMutable { get; }
    }
}
