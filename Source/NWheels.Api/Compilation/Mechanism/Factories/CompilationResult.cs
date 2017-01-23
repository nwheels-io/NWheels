using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Factories
{
    public class CompilationResult<TArtifact>
    {
        public CompilationResult(
            bool success, 
            IReadOnlyList<TypeCompilationResult<TArtifact>> allTypes, 
            IReadOnlyList<TypeCompilationResult<TArtifact>> typesWithIssues)
        {
            this.Success = success;
            this.AllTypes = allTypes;
            this.TypesWithIssues = typesWithIssues;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool Success { get; }
        public IReadOnlyList<TypeCompilationResult<TArtifact>> AllTypes { get; }
        public IReadOnlyList<TypeCompilationResult<TArtifact>> TypesWithIssues { get; }
    }
}
