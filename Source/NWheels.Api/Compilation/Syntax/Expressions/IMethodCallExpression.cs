using NWheels.Api.Compilation.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Api.Compilation.Syntax.Expressions
{
    public interface IMethodCallExpression : IExpression
    {
        IMethodMember Method { get; }
        IReadOnlyList<IExpression> Arguments { get; }
    }
}
