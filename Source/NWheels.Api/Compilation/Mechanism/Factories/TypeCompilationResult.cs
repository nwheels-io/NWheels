using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace NWheels.Compilation.Mechanism.Factories
{
    public abstract class TypeCompilationResult
    {
        public abstract TypeMember Type { get; }
        public abstract bool Success { get; }
        public abstract IReadOnlyList<CompilationIssue> Issues { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class TypeCompilationResult<TArtifact> : TypeCompilationResult
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

        public override TypeMember Type { get; }
        public override bool Success { get; }
        public override IReadOnlyList<CompilationIssue> Issues { get; }
        public TArtifact Artifact { get; }
    }
}
