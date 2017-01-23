using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Factories
{
    public interface ITypeFactoryBackend<TArtifact>
    {
        CompilationResult<TArtifact> Compile(IEnumerable<TypeMember> types);
        event Action<TypeFactoryProduct<TArtifact>[]> ProductsLoaded;
    }
}
