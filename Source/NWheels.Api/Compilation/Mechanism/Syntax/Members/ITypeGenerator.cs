using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Members
{
    public interface ITypeGenerator
    {
        IReadOnlyList<TypeMember> ImplementInterface(TypeMember interfaceType, bool isExplicit);
        TypeMember Product { get; }
    }
}
