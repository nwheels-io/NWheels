using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax
{
    public abstract class MemberVisitor
    {
        public virtual void VisitClassType(TypeMember type)
        {
            VisitTypeMember(type);
        }

        public virtual void VisitStructType(TypeMember type)
        {
            VisitTypeMember(type);
        }

        public virtual void VisitInterfaceType(TypeMember type)
        {
            VisitTypeMember(type);
        }

        public virtual void VisitEnumType(TypeMember type)
        {
            VisitTypeMember(type);
        }

        public virtual void VisitConstructor(ConstructorMember constructor)
        {
            VisitMethodBase(constructor);
        }

        public virtual void VisitMethod(MethodMember method)
        {
            VisitMethodBase(method);
        }

        public virtual void VisitProperty(PropertyMember property)
        {
            VisitAbstractMember(property);
        }

        public virtual void VisitEvent(EventMember eventMember)
        {
            VisitAbstractMember(eventMember);
        }

        public virtual void VisitField(FieldMember field)
        {
            VisitAbstractMember(field);
        }

        public virtual void VisitEnumMember(EnumMember member)
        {
            VisitAbstractMember(member);
        }

        public virtual void VisitAttribute(AttributeDescription attribute)
        {
        }

        internal protected virtual void VisitAbstractMember(AbstractMember member)
        {
        }

        internal protected virtual void VisitTypeMember(TypeMember type)
        {
            VisitAbstractMember(type);
        }

        internal protected virtual void VisitMethodBase(MethodMemberBase method)
        {
            VisitAbstractMember(method);
        }
    }
}
