using NWheels.Api.Compilation.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Api.Compilation.Conventions
{
    public interface ITypeFactoryContext
    {
        ITypeKey Key { get; }
        ITypeMemberBuilder Builder { get; }
    }

    public interface ITypeFactoryContext<TExtension> : ITypeFactoryContext
    {
        TExtension Extension { get; }
    }
}
