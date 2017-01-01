using NWheels.Api.Compilation.Syntax.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Api.Compilation.Syntax.Members
{
    public interface IConstructorMember : IMethodMemberBase
    {
        IMethodCallExpression CallThisConstructor { get; }
        IMethodCallExpression CallBaseConstructor { get; }
    }
}
