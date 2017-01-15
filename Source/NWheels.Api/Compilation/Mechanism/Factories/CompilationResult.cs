using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Factories
{
    public class CompilationResult<TArtifact>
    {
        public bool Success { get; }
        public IReadOnlyList<TypeCompilationResult<TArtifact>> Types { get; }
        public IReadOnlyList<CompilationIssue> Issues { get; }
    }
}
