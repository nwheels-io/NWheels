using NWheels.Api.Compilation.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Api.Compilation.Conventions
{
    public interface ITypeKey
    {
        ITypeMember PrimaryContract { get; }
        IReadOnlyList<ITypeMember> SecondaryContracts { get; }
    }

    public interface ITypeKey<TTag> : ITypeKey
    {
        TTag Tag { get; }
    }
}
