using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Members
{
    public class EnumMember : AbstractMember
    {
        public override void AcceptVisitor(MemberVisitor visitor)
        {
            visitor.VisitEnumMember(this);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public object Value { get; set; }
    }
}
