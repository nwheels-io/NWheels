using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Factories
{
    public class CompilationResult
    {
        public CompilationResult(
            IReadOnlyList<TypeCompilationResult> succeeded, 
            IReadOnlyList<TypeCompilationResult> failed,
            IReadOnlyList<CompilationDiagnostic> globalDiagnostics)
        {
            this.Succeeded = succeeded;
            this.Failed = failed;
            this.GlobalDiagnostics = globalDiagnostics;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool Success => (Failed.Count == 0);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IReadOnlyList<TypeCompilationResult> Succeeded { get; }
        public IReadOnlyList<TypeCompilationResult> Failed { get; }
        public IReadOnlyList<CompilationDiagnostic> GlobalDiagnostics { get; }
    }
}
