using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Factories
{
    public class CompilationIssue
    {
        public CompilationIssueSeverity Severity { get; }
        public string Code { get; }
        public string Message { get; }
        public string SourceLocation { get; }
    }

    public enum CompilationIssueSeverity
    {
        Verbose,
        Info,
        Warning,
        Error
    }
}
