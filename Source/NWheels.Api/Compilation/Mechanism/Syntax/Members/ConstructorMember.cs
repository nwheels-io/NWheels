using NWheels.Compilation.Mechanism.Syntax.Expressions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Members
{
    public class ConstructorMember : MethodMemberBase
    {
        public ConstructorMember(
            MemberVisibility visibility,
            MemberModifier modifier,
            string name,
            MethodSignature signature)
        {
            this.Visibility = visibility;
            this.Modifier = modifier;
            this.Name = name;
            this.Signature = signature;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ConstructorMember(ConstructorInfo clrBinding)
            : base(clrBinding)
        {
            this.Name = Name;
            this.Signature = new MethodSignature(clrBinding);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void AcceptVisitor(MemberVisitor visitor)
        {
            base.AcceptVisitor(visitor);
            visitor.VisitConstructor(this);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MethodCallExpression CallThisConstructor { get; set; }
        public MethodCallExpression CallBaseConstructor { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ConstructorInfo ClrBinding { get; set; }
    }
}
