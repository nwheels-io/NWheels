using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax
{
    public class TypeReferenceStatementVisitor : StatementVisitor
    {
        private readonly HashSet<TypeMember> _referencedTypes;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeReferenceStatementVisitor(HashSet<TypeMember> referencedTypes)
        {
            _referencedTypes = referencedTypes;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

    }
}
