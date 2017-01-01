using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Api.Compilation.Syntax.Expressions
{
    public interface IIndexerExpression : IExpression
    {
        IExpression Target { get; }
        IReadOnlyList<IExpression> IndexArguments { get; }
    }
}
