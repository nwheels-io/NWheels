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
using Microsoft.CodeAnalysis;

namespace NWheels.Compilation.Adapters.Roslyn
{
    public class RoslynTypeFactoryBackend : ITypeFactoryBackend<IRuntimeTypeFactoryArtifact>
    {
        private readonly bool _debugMode;
        private readonly string _assemblyNamePrefix;
        private readonly string _generatedSourceDirectory;
        private readonly string _compiledAssemblyDirectory;
        private readonly ReferenceCache _referenceCache;
        private readonly List<Assembly> _compiledAssemblies;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public RoslynTypeFactoryBackend()
            : this(
                  debugMode: true, 
                  assemblyNamePrefix: "RunTimeTypes",
                  generatedSourceDirectory: GetDefaultArtifactDirectory(),
                  compiledAssemblyDirectory: GetDefaultArtifactDirectory())
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public RoslynTypeFactoryBackend(bool debugMode, string assemblyNamePrefix, string generatedSourceDirectory, string compiledAssemblyDirectory)
        {
            _debugMode = debugMode;
            _assemblyNamePrefix = assemblyNamePrefix;
            _generatedSourceDirectory = generatedSourceDirectory;
            _compiledAssemblyDirectory = compiledAssemblyDirectory;

            _referenceCache = new ReferenceCache();
            _compiledAssemblies = new List<Assembly>();

            _referenceCache.IncludeSystemAssemblyReferences();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CompilationResult Compile(IEnumerable<TypeMember> types)
        {
            var generator = new SourceCodeGenerator();
            var syntax = generator.GenerateSyntax(types);

            var assembly = TryCompileNewAssembly(syntax.SyntaxTree);

            //TODO: create real results
            var result = new CompilationResult(
                succeeded: types.Select(t => 
                    new TypeCompilationResult<IRuntimeTypeFactoryArtifact>(
                        t, 
                        true, 
                        new RuntimeTypeFactoryArtifact(null), new List<CompilationIssue>())).ToList(),
                failed: new List<TypeCompilationResult<IRuntimeTypeFactoryArtifact>>());

            //TODO: raise ArtifactsLoaded event

            return result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeMember GetBoundTypeMember(TypeFactoryProduct<IRuntimeTypeFactoryArtifact> product)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void LoadPrecompiledAssembly(string filePath)
        {
            ProductsLoaded?.Invoke(null);
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IReadOnlyList<Assembly> CompiledAssemblies => _compiledAssemblies;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public event Action<TypeFactoryProduct<IRuntimeTypeFactoryArtifact>[]> ProductsLoaded;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Assembly TryCompileNewAssembly(SyntaxTree syntax)
        {
            var compiler = new AssemblyCompiler(_referenceCache);
            var assemblyName = $"{_assemblyNamePrefix}{_compiledAssemblies.Count + 1}";

            if (_debugMode)
            {
                var sourceFilePath = Path.Combine(_generatedSourceDirectory, assemblyName + ".cs");
                File.WriteAllText(sourceFilePath, syntax.ToString());
                syntax = syntax.WithFilePath(sourceFilePath);
            }

            if (compiler.CompileAssembly(
                syntax,
                references: new string[0],
                enableDebug: _debugMode,
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static string GetDefaultArtifactDirectory()
        {
            return Path.GetDirectoryName(typeof(RoslynTypeFactoryBackend).GetTypeInfo().Assembly.Location);
        }
    }
}
