using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Factories
{
    public interface ITypeFactoryBackend<TArtifact>
    {
        CompilationResult Compile(IEnumerable<TypeMember> types);
        TypeMember GetBoundTypeMember(TypeFactoryProduct<TArtifact> product);
        event Action<TypeFactoryProduct<TArtifact>[]> ProductsLoaded;
    }
}
