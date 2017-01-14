using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Expressions
{
    public class NewArrayExpression : AbstractExpression
    {
        public NewArrayExpression()
        {
            this.DimensionLengths = new List<AbstractExpression>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public AbstractExpression Length
        {
            get
            {
                if (DimensionLengths.Count == 0)
                {
                    throw new InvalidOperationException("Dimension lengths were not set");
                }

                if (DimensionLengths.Count != 1)
                {
                    throw new InvalidOperationException("This is a multi-dimensional array");
                }

                return DimensionLengths[0];
            }
            set
            {
                DimensionLengths.Clear();
                DimensionLengths.Add(value);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeMember ElementType { get; set; }
        public List<AbstractExpression> DimensionLengths { get; set; }
    }
}
