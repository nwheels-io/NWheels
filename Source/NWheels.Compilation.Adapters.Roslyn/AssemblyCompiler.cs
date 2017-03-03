using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Reflection;

namespace NWheels.Compilation.Adapters.Roslyn
{
    public class AssemblyCompiler
    {
        private readonly ReferenceCache _referenceCache;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public AssemblyCompiler(ReferenceCache referenceCache)
        {
            _referenceCache = referenceCache;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EmitResult CompileAssembly(
            SyntaxTree code,
            string[] references,
            bool enableDebug,
            string assemblyName,
            out byte[] dllBytes,
            out byte[] pdbBytes)
        {
            var context = new CompilationContext {
                AssemblyName = assemblyName,
                SourceCode = code,
                ReferencePaths = references,
                EnableDebug = enableDebug
            };

            LoadReferences(context);
            CreateCompilation(context);
            EmitAssembly(context);

            dllBytes = context.DllBytes;
            pdbBytes = context.PdbBytes;
            return context.Result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void LoadReferences(CompilationContext context)
        {
            var references = new MetadataReference[context.ReferencePaths.Length];

            for (int i = 0 ; i < references.Length ; i++)
            {
                references[i] = _referenceCache.EnsureReferenceCached(context.ReferencePaths[i]);
            }

            context.LoadedReferences = _referenceCache.GetAllCachedReferences().ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void CreateCompilation(CompilationContext context)
        {
            context.Compilation = CSharpCompilation
                .Create(context.AssemblyName, options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(context.LoadedReferences)
                .AddSyntaxTrees(context.SourceCode);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void EmitAssembly(CompilationContext context)
        {
            using (var dllStream = new MemoryStream())// (capacity: 16384))
            {
                using (var pdbStream = context.EnableDebug ? new MemoryStream() : null)
                {
                    context.Result = context.Compilation.Emit(
                        dllStream, 
                        pdbStream, 
                        options: new EmitOptions(debugInformationFormat: DebugInformationFormat.PortablePdb));

                    context.DllBytes = dllStream.ToArray();
                    context.PdbBytes = pdbStream?.ToArray();
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        //private bool IsErrorOrWarning(Diagnostic diagnostic)
        //{
        //    return (diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);
        //}

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class CompilationContext
        {
            public SyntaxTree SourceCode { get; set; }
            public string[] ReferencePaths { get; set; }
            public bool EnableDebug { get; set; }
            public string AssemblyName { get; set; }
            public MetadataReference[] LoadedReferences { get; set; }
            public CSharpCompilation Compilation { get; set; }
            public EmitResult Result { get; set; }
            public byte[] DllBytes { get; set; }
            public byte[] PdbBytes { get; set; }
        }
    }
}
