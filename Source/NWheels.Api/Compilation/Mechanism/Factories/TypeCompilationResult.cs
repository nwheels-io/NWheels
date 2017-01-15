using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace NWheels.Compilation.Mechanism.Factories
{
    public class TypeCompilationResult<TArtifact>
    {
        public TypeMember Type { get; }
        public bool Success { get; }
        public TArtifact Artifact { get; }
        public IReadOnlyList<CompilationIssue> Issues { get; }
    }
}
