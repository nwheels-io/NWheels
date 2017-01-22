using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Factories
{
    public interface ITypeFactoryContext
    {
        ITypeKey Key { get; }
        object Extension { get; }
        TypeMember Product { get; }
    }

    public interface ITypeFactoryContext<TKeyExtension, TContextExtension>
        where TKeyExtension : ITypeKeyExtension, new()
    {
        ITypeKey<TKeyExtension> Key { get; }
        TContextExtension Extension { get; }
        TypeMember Product { get; }
    }
}
