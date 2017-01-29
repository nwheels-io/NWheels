using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace NWheels.Compilation.Mechanism.Factories
{
    public abstract class TypeCompilationResult
    {
        public abstract TypeKey Key { get; }
        public abstract TypeMember Type { get; }
        public abstract bool Success { get; }
        public abstract IReadOnlyList<CompilationDiagnostic> Diagnostics { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class TypeCompilationResult<TArtifact> : TypeCompilationResult
    {
        public TypeCompilationResult(
            TypeMember type,
            bool success,
            TArtifact artifact,
            IReadOnlyList<CompilationDiagnostic> diagnostics)
        {
            this.Type = type;
            this.Success = success;
            this.Artifact = artifact;
            this.Diagnostics = diagnostics;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override TypeKey Key => Type.Generator.TypeKey;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override TypeMember Type { get; }
        public override bool Success { get; }
        public override IReadOnlyList<CompilationDiagnostic> Diagnostics { get; }
        public TArtifact Artifact { get; }
    }
}
