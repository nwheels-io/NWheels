using NWheels.Compilation.Mechanism.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NWheels.Exceptions
{
    public class CompilationErrorsException : Exception
    {
        public CompilationErrorsException(IEnumerable<TypeCompilationResult> typesFailedToCompile)
            : base("Compilation of some type members failed. Examine TypesFailedToCompile collection to find errors.")
        {
            TypesFailedToCompile = typesFailedToCompile.ToList();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IReadOnlyList<TypeCompilationResult> TypesFailedToCompile { get; }
    }
}
