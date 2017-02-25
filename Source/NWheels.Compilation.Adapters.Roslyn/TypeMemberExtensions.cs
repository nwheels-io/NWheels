using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Adapters.Roslyn
{
    public static class TypeMemberExtensions
    {
        internal static RoslynTypeFactoryBackend.BackendTag SafeBackendTag(this TypeMember type)
        {
            return TypeMemberTagCache.Current.GetOrAddBackendTagFor(type);
        }
    }
}
