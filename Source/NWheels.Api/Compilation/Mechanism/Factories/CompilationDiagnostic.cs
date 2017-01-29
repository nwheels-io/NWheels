using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Factories
{
    public class CompilationDiagnostic
    {
        public CompilationDiagnostic(CompilationDiagnosticSeverity severity, string code, string message, string sourceLocation)
        {
            this.Severity = severity;
            this.Code = code;
            this.Message = message;
            this.SourceLocation = sourceLocation;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CompilationDiagnosticSeverity Severity { get; }
        public string Code { get; }
        public string Message { get; }
        public string SourceLocation { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum CompilationDiagnosticSeverity
    {
        Verbose,
        Info,
        Warning,
        Error
    }
}
