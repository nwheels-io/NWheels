using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace NWheels.Compilation.Mechanism.Factories
{
    public class TypeCompilationResult<TArtifact>
    {
        public TypeCompilationResult(
            TypeMember type,
            bool success,
            TArtifact artifact,
            IReadOnlyList<CompilationIssue> issues)
        {
            this.Type = type;
            this.Success = success;
            this.Artifact = artifact;
            this.Issues = issues;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeMember Type { get; }
        public bool Success { get; }
        public TArtifact Artifact { get; }
        public IReadOnlyList<CompilationIssue> Issues { get; }
    }
}
