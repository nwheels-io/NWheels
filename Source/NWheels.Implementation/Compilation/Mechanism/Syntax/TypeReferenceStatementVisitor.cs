using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Compilation.Mechanism.Syntax.Statements;

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

        public override void VisitVariableDeclaraitonStatement(VariableDeclarationStatement statement)
        {
            base.VisitVariableDeclaraitonStatement(statement);
            TypeReferenceMemberVisitor.AddReferencedType(_referencedTypes, statement.Variable.Type);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------


    }
}
