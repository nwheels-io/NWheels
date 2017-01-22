using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Factories
{
    public interface ITypeKey
    {
        TypeMember PrimaryContract { get; }
        IReadOnlyList<TypeMember> SecondaryContracts { get; }
        object Extension { get; }
    }

    public interface ITypeKey<TExtension>
        where TExtension : ITypeKeyExtension, new()
    {
        TypeMember PrimaryContract { get; }
        IReadOnlyList<TypeMember> SecondaryContracts { get; }
        TExtension Extension { get; }
    }
}
