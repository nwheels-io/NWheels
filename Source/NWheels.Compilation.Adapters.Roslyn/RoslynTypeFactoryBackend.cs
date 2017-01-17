using NWheels.Compilation.Mechanism.Factories;
using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NWheels.Compilation.Adapters.Roslyn
{
    public class RoslynTypeFactoryBackend : ITypeFactoryBackend<IRuntimeTypeFactoryArtifact>
    {
        private readonly ReferenceCache _referenceCache;
        private readonly string _compiledAssemblyDirectory;
        private readonly List<Assembly> _compiledAssemblies;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public RoslynTypeFactoryBackend()
        {
            _referenceCache = new ReferenceCache();
            _compiledAssemblyDirectory = Path.GetDirectoryName(System.Environment.GetCommandLineArgs()[0]);
            _compiledAssemblies = new List<Assembly>();

            _referenceCache.IncludeSystemAssemblyReferences();
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
            var generator = new SourceCodeGenerator();
            var syntax = generator.GenerateSyntax(types);

            var assembly = TryCompileNewAssembly(syntax);

            return new CompilationResult<IRuntimeTypeFactoryArtifact>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IReadOnlyList<Assembly> CompiledAssemblies => _compiledAssemblies;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Assembly TryCompileNewAssembly(CompilationUnitSyntax syntax)
        {
            var compiler = new AssemblyCompiler(_referenceCache);
            var assemblyName = $"RunTimeTypes{_compiledAssemblies.Count + 1}";

            if (compiler.CompileAssembly(
                syntax,
                references: new string[0],
                enableDebug: true,
                assemblyName: assemblyName,
                dllBytes: out byte[] dllBytes,
                pdbBytes: out byte[] pdbBytes,
                errors: out string[] errors))
            {
                File.WriteAllBytes(Path.Combine(_compiledAssemblyDirectory, assemblyName + ".dll"), dllBytes);

                if (pdbBytes != null)
                {
                    File.WriteAllBytes(Path.Combine(_compiledAssemblyDirectory, assemblyName + ".pdb"), pdbBytes);
                }

                var assembly = Assembly.Load(new AssemblyName(assemblyName));
                _compiledAssemblies.Add(assembly);
                return assembly;
            }

            return null;
        }
    }
}
