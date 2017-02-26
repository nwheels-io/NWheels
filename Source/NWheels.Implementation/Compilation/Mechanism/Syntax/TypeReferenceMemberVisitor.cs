using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax
{
    public class TypeReferenceMemberVisitor : MemberVisitor
    {
        private readonly HashSet<TypeMember> _referencedTypes;
        private readonly TypeReferenceStatementVisitor _statementVisitor;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeReferenceMemberVisitor(HashSet<TypeMember> referencedTypes)
        {
            _referencedTypes = referencedTypes;
            _statementVisitor = new TypeReferenceStatementVisitor(referencedTypes);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void VisitTypeMember(TypeMember type)
        {
            base.VisitTypeMember(type);
            AddReferencedType(type);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void VisitMethodBase(MethodMemberBase method)
        {
            base.VisitMethodBase(method);

            if (method.Signature != null )
            {
                AddReferencedType(method.Signature.ReturnValue?.Type);

                if (method.Signature.Parameters != null)
                {
                    foreach (var parameter in method.Signature.Parameters)
                    {
                        AddReferencedType(parameter.Type);
                    }
                }
            }

            if (method.Body != null)
            {
                method.Body.AcceptVisitor(_statementVisitor);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void VisitField(FieldMember field)
        {
            base.VisitField(field);
            AddReferencedType(field.Type);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void VisitProperty(PropertyMember property)
        {
            base.VisitProperty(property);
            AddReferencedType(property.PropertyType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void VisitEvent(EventMember eventMember)
        {
            base.VisitEvent(eventMember);
            AddReferencedType(eventMember.DelegateType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AddReferencedType(TypeMember type)
        {
            AddReferencedType(_referencedTypes, type);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal static void AddReferencedType(HashSet<TypeMember> typeSet, TypeMember type)
        {
            if (type != null)
            {
                if (!type.IsGenericType || type.IsGenericTypeDefinition)
                {
                    typeSet.Add(type);
                }

                if (type.IsGenericType && !type.IsGenericTypeDefinition) 
                {
                    AddReferencedType(typeSet, type.GenericTypeDefinition);

                    foreach (var argument in type.GenericTypeArguments)
                    {
                        AddReferencedType(typeSet, argument);
                    }
                }
            }
        }
    }
}
