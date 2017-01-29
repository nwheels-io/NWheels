using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax
{
    public abstract class MemberSyntaxVisitor
    {
        public virtual void VisitAbstractMember(AbstractMember member)
        {
        }

        public virtual void VisitTypeMember(TypeMember type)
        {
        }

        public virtual void VisitClassType(TypeMember type)
        {
        }

        public virtual void VisitStructType(TypeMember type)
        {
        }

        public virtual void VisitEnumType(TypeMember type)
        {
        }

        public virtual void VisitMethodBase(MethodMemberBase methodBase)
        {
        }

        public virtual void VisitConstructor(ConstructorMember constructor)
        {
        }

        public virtual void VisitMethod(MethodMember method)
        {
        }

        public virtual void VisitProperty(PropertyMember property)
        {
        }

        public virtual void VisitEvent(EventMember eventMember)
        {
        }

        public virtual void VisitField(FieldMember field)
        {
        }

        public virtual void VisitEnumMember(EnumMember member)
        {
        }

        public virtual void VisitAttribute(AttributeDescription attribute)
        {
        }
    }
}
