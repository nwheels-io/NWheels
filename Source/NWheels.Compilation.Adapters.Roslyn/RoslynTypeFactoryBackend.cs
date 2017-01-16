using NWheels.Compilation.Mechanism.Factories;
using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System.Linq;
using System.Reflection;

namespace NWheels.Compilation.Adapters.Roslyn
{
    public class RoslynTypeFactoryBackend : ITypeFactoryBackend<IRuntimeTypeFactoryArtifact>
    {
        private readonly ReferenceCache _referenceCache;
        private readonly List<Assembly> _compiledAssemblies;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public RoslynTypeFactoryBackend()
        {
            _referenceCache = new ReferenceCache();
            _compiledAssemblies = new List<Assembly>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeCompilationResult<IRuntimeTypeFactoryArtifact> CompileSingleType(TypeMember type)
        {
            var result = CompileMultipleTypes(new[] { type });
            return result.Types.Single();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CompilationResult<IRuntimeTypeFactoryArtifact> CompileMultipleTypes(IEnumerable<TypeMember> types)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IReadOnlyList<Assembly> CompiledAssemblies => _compiledAssemblies;
    }
}
