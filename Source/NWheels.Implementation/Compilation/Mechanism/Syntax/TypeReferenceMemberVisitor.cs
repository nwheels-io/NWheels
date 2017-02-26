using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax
{
    public class TypeReferenceMemberVisitor : MemberVisitor
    {
        private readonly HashSet<TypeMember> _referencedTypes;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeReferenceMemberVisitor(HashSet<TypeMember> referencedTypes)
        {
            _referencedTypes = referencedTypes;
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

        private void AddReferencedType(TypeMember type)
        {
            if (type != null)
            {
                _referencedTypes.Add(type);
            }
        }
    }
}
