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
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using NWheels.DataStructures;

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

            _referenceCache.IncludePrerequisiteAssemblyReferences();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CompilationResult Compile(ICollection<TypeMember> types)
        {
            var catalogBuilder = new AssemblyArtifactCatalogBuilder(types);
            catalogBuilder.BuildArtifactCatalog();

            var allTypes = types.Concat(catalogBuilder.CatalogTypes).ToList();

            var syntaxGenerator = new CSharpSyntaxGenerator();
            var syntaxTree = syntaxGenerator.GenerateSyntax(allTypes);
            var success = TryCompileNewAssembly(ref syntaxTree, out Assembly assembly, out ImmutableArray<Diagnostic> diagnostics);

            if (success)
            {
                var artifactPerTypeKey = LoadProductAssembly(assembly);
                var productPerType = CreateProductPerTypeDictionary(types, artifactPerTypeKey);

                ProductsLoaded?.Invoke(productPerType.Values.ToArray());

                return CreateSuccessfulCompilationResult(types, diagnostics, productPerType);
            }

            return CreateFailedCompilatioResult(types, diagnostics, syntaxTree);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void EnsureTypeReferenced(Type type)
        {
            _referenceCache.EnsureReferenceCached(type.GetTypeInfo().Assembly.Location);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void LoadPrecompiledAssembly(string filePath)
        {
            var name = new AssemblyName(Path.GetFileNameWithoutExtension(filePath));
            var assembly = Assembly.Load(name);

            _compiledAssemblies.Add(assembly);

            var artifactPerTypeKey = LoadProductAssembly(assembly);
            var loadedProducts = artifactPerTypeKey
                .Select(keyArtifactPair => new TypeFactoryProduct<IRuntimeTypeFactoryArtifact>(
                    keyArtifactPair.Key,
                    keyArtifactPair.Value))
                .ToArray();

            ProductsLoaded?.Invoke(loadedProducts);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeMember GetBoundTypeMember(TypeFactoryProduct<IRuntimeTypeFactoryArtifact> product)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IReadOnlyList<Assembly> CompiledAssemblies => _compiledAssemblies;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public event Action<TypeFactoryProduct<IRuntimeTypeFactoryArtifact>[]> ProductsLoaded;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool TryCompileNewAssembly(ref SyntaxTree syntax, out Assembly assembly, out ImmutableArray<Diagnostic> diagnostics)
        {
            var compiler = new AssemblyCompiler(_referenceCache);
            var assemblyName = $"{_assemblyNamePrefix}{_compiledAssemblies.Count + 1}";

            if (_debugMode)
            {
                var sourceFilePath = Path.Combine(_generatedSourceDirectory, assemblyName + ".cs");
                File.WriteAllText(sourceFilePath, syntax.ToString(), Encoding.UTF8);
                syntax = syntax.WithFilePath(sourceFilePath);
            }

            var result = compiler.CompileAssembly(
                syntax,
                references: new string[0],
                enableDebug: _debugMode,
                assemblyName: assemblyName,
                dllBytes: out byte[] dllBytes,
                pdbBytes: out byte[] pdbBytes);

            if (result.Success)
            {
                File.WriteAllBytes(Path.Combine(_compiledAssemblyDirectory, assemblyName + ".dll"), dllBytes);

                if (pdbBytes != null)
                {
                    File.WriteAllBytes(Path.Combine(_compiledAssemblyDirectory, assemblyName + ".pdb"), pdbBytes);
                }

                assembly = Assembly.Load(new AssemblyName(assemblyName));
                _compiledAssemblies.Add(assembly);
            }
            else
            {
                assembly = null;
            }

            diagnostics = result.Diagnostics;
            return result.Success;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Dictionary<TypeKey, RuntimeTypeFactoryArtifact> LoadProductAssembly(Assembly assembly)
        {
            var catalog = RuntimeTypeFactoryArtifactCatalog.LoadFrom(assembly);
            var artifacts = catalog.GetArtifacts();
            var artifactByTypeKey = artifacts.ToDictionary(a => a.TypeKey);

            return artifactByTypeKey;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private TextSpan GetTaggedTypeSyntaxSpan(SyntaxTree tree, TypeMember type)
        {
            if (type.BackendTag is SyntaxAnnotation annotation)
            {
                var typeSyntax = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .Where(n => n.HasAnnotation(annotation))
                    .FirstOrDefault();

                if (typeSyntax != null)
                {
                    return typeSyntax.FullSpan;
                }
            }

            return TextSpan.FromBounds(0, 0);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Dictionary<TypeMember, TypeFactoryProduct<IRuntimeTypeFactoryArtifact>> CreateProductPerTypeDictionary(
            ICollection<TypeMember> types,
            Dictionary<TypeKey, RuntimeTypeFactoryArtifact> artifactPerTypeKey)
        {
            var typePerKey = types
                .Where(t => t.Generator.TypeKey.HasValue)
                .ToDictionary(t => t.Generator.TypeKey.Value, t => t);

            var productPerType = new Dictionary<TypeMember, TypeFactoryProduct<IRuntimeTypeFactoryArtifact>>();

            foreach (var keyTypePair in typePerKey)
            {
                var artifact = artifactPerTypeKey[keyTypePair.Key];

                productPerType.Add(
                    keyTypePair.Value,
                    new TypeFactoryProduct<IRuntimeTypeFactoryArtifact>(keyTypePair.Key, artifact));
            }

            return productPerType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private CompilationResult CreateSuccessfulCompilationResult(
            ICollection<TypeMember> types, 
            ImmutableArray<Diagnostic> diagnostics, 
            Dictionary<TypeMember, TypeFactoryProduct<IRuntimeTypeFactoryArtifact>> productPerType)
        {
            var result = new CompilationResult(
                succeeded: types.Select(t => new TypeCompilationResult<IRuntimeTypeFactoryArtifact>(
                    type: t,
                    success: true,
                    artifact: productPerType[t].Artifact,
                    diagnostics: _s_emptyDiagnostics)).ToList(),
                failed: _s_emptyCompilationResults,
                globalDiagnostics: diagnostics.Select(ToCompilationDiagnostic).ToList());

            return result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private CompilationResult CreateFailedCompilatioResult(ICollection<TypeMember> types, ImmutableArray<Diagnostic> diagnostics, SyntaxTree syntaxTree)
        {
            List<TypeDiagnostics> typeDiagnostics;
            List<CompilationDiagnostic> globalDiagnostics;
            SortDiagnosticsOut(types, syntaxTree, diagnostics, out typeDiagnostics, out globalDiagnostics);

            var result = new CompilationResult(
                succeeded: _s_emptyCompilationResults,
                failed: typeDiagnostics.Select(tdPair => new TypeCompilationResult<IRuntimeTypeFactoryArtifact>(
                    type: tdPair.Type,
                    success: tdPair.IsSuccess(),
                    artifact: null,
                    diagnostics: tdPair.Diagnostics)).ToList(),
                globalDiagnostics: globalDiagnostics);

            return result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void SortDiagnosticsOut(
            ICollection<TypeMember> types,
            SyntaxTree syntaxTree,
            ImmutableArray<Diagnostic> diagnostics,
            out List<TypeDiagnostics> typeDiagnostics,
            out List<CompilationDiagnostic> globalDiagnostics)
        {
            globalDiagnostics = new List<CompilationDiagnostic>();
            typeDiagnostics = new List<TypeDiagnostics>(capacity: types.Count);

            typeDiagnostics.AddRange(types.Select(t => new TypeDiagnostics(t, GetTaggedTypeSyntaxSpan(syntaxTree, t))));
            typeDiagnostics.Sort((x, y) => x.FullSpan.Start.CompareTo(y.FullSpan.Start));

            var typeSpans = typeDiagnostics.Select(tdPair => tdPair.FullSpan).ToList();

            foreach (var diagnostic in diagnostics)
            {
                var index = typeSpans.BinarySearch(diagnostic.Location.SourceSpan, _s_textSpanComparer);

                if (index >= 0 && index < typeSpans.Count && typeSpans[index].IntersectsWith(diagnostic.Location.SourceSpan))
                {
                    typeDiagnostics[index].Diagnostics.Add(ToCompilationDiagnostic(diagnostic));
                }
                else
                {
                    globalDiagnostics.Add(ToCompilationDiagnostic(diagnostic));
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private CompilationDiagnostic ToCompilationDiagnostic(Diagnostic roslynDiagnostic)
        {
            return new CompilationDiagnostic(
                severity: ToCompilationDiagnosticSeverity(roslynDiagnostic.Severity),
                code: roslynDiagnostic.Id,
                message: roslynDiagnostic.GetMessage(),
                sourceLocation: roslynDiagnostic.Location?.ToString());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private CompilationDiagnosticSeverity ToCompilationDiagnosticSeverity(DiagnosticSeverity severity)
        {
            switch (severity)
            {
                case DiagnosticSeverity.Error:
                    return CompilationDiagnosticSeverity.Error;
                case DiagnosticSeverity.Warning:
                    return CompilationDiagnosticSeverity.Warning;
                case DiagnosticSeverity.Info:
                    return CompilationDiagnosticSeverity.Info;
                default:
                    return CompilationDiagnosticSeverity.Verbose;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly IReadOnlyList<CompilationDiagnostic> _s_emptyDiagnostics = new CompilationDiagnostic[0];
        private static readonly IReadOnlyList<TypeCompilationResult> _s_emptyCompilationResults = new TypeCompilationResult[0];
        private static readonly TextSpanComparer _s_textSpanComparer = new TextSpanComparer();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static string GetDefaultArtifactDirectory()
        {
            return Path.GetDirectoryName(typeof(RoslynTypeFactoryBackend).GetTypeInfo().Assembly.Location);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private struct TypeDiagnostics
        {
            public TypeDiagnostics(TypeMember type, TextSpan fullSpan)
            {
                this.Type = type;
                this.FullSpan = fullSpan;
                this.Diagnostics = new List<CompilationDiagnostic>();
            }
            public bool IsSuccess()
            {
                return !Diagnostics.Any(d => d.Severity == CompilationDiagnosticSeverity.Error);
            }
            public readonly TypeMember Type;
            public readonly TextSpan FullSpan;
            public readonly List<CompilationDiagnostic> Diagnostics;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class TextSpanComparer : IComparer<TextSpan>
        {
            public int Compare(TextSpan x, TextSpan y)
            {
                if (x.IntersectsWith(y))
                {
                    return 0;
                }

                return x.Start.CompareTo(y.Start);
            }
        }
    }
}
