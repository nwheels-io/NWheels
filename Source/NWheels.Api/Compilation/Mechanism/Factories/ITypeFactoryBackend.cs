using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Factories
{
    public interface ITypeFactoryBackend<TArtifact>
    {
        TypeCompilationResult<TArtifact> CompileSingleType(TypeMember syntax);
        CompilationResult<TArtifact> CompileMultipleTypes(IEnumerable<TypeMember> syntaxes);
    }
}
