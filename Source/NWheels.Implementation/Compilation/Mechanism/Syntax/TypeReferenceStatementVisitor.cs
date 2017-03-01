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
            AddReferencedType(statement.Variable?.Type);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public override void VisitReferenceToTypeMember(TypeMember type)
        {
            base.VisitReferenceToTypeMember(type);
            AddReferencedType(type);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public override void VisitReferenceToLocalVariable(LocalVariable variable)
        {
            base.VisitReferenceToLocalVariable(variable);
            AddReferencedType(variable.Type);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private void AddReferencedType(TypeMember type)
        {
            TypeReferenceMemberVisitor.AddReferencedType(_referencedTypes, type);
        }
    }
}
