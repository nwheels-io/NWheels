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
            if (type.BackendTag is RoslynTypeFactoryBackend.BackendTag existingTag)
            {
                return existingTag;
            }

            var newTag = new RoslynTypeFactoryBackend.BackendTag();
            type.BackendTag = newTag;
            return newTag;
        }
    }
}
