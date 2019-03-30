#if false

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NWheels.Build
{
    
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class IntentAsCodeAnalyzer : DiagnosticAnalyzer
    {
        internal static DiagnosticDescriptor Rule =
            new DiagnosticDescriptor(
                "NW002",
                title: "Transpile intent-as-code",
                messageFormat: "NWheels: transpiling intent-as-code, input project '{0}'",
                category: "NWheels Build",
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true,
                description: "Transpile intent-as-code");        
        
        public override void Initialize(AnalysisContext context)
        {
            Console.WriteLine($"\r\n---- PROCESS ID {Process.GetCurrentProcess().Id} ---- WAITING FOR DEBUGGER ----\r\n");
            Thread.Sleep(30000);
            
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationAction(AnalyzeCompilation);
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        private static void AnalyzeCompilation(CompilationAnalysisContext context)
        {
            Diagnostic diagnostic = Diagnostic.Create(Rule, Location.None, context.Compilation.Assembly.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}

#endif