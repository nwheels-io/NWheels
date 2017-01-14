using NWheels.Compilation.Mechanism.Syntax.Expressions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Members
{
    public class ConstructorMember : MethodMemberBase
    {
        public MethodCallExpression CallThisConstructor { get; set; }
        public MethodCallExpression CallBaseConstructor { get; set; }
        public ConstructorInfo Binding { get; set; }
    }
}
