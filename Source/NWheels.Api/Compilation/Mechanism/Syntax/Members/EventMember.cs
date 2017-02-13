using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Members
{
    public class EventMember : AbstractMember
    {
        public EventMember()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EventMember(MemberVisibility visibility, MemberModifier modifier, TypeMember delegateType, string name)
            : base(visibility, modifier, name)
        {
            this.DelegateType = delegateType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EventMember(EventInfo clrBinding)
            : base(clrBinding)
        {
            this.ClrBinding = ClrBinding;
            this.DelegateType = clrBinding.DeclaringType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void AcceptVisitor(MemberVisitor visitor)
        {
            visitor.VisitEvent(this);

            if (Adder != null)
            {
                Adder.AcceptVisitor(visitor);
            }

            if (Remover != null)
            {
                Remover.AcceptVisitor(visitor);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeMember DelegateType { get; set; }
        public MethodMember Adder { get; set; }
        public MethodMember Remover { get; set; }
        public EventInfo ClrBinding { get; set; }
    }
}
