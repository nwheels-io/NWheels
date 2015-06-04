using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class CodeBehindAttribute : Attribute
    {
        public CodeBehindAttribute(Type codeBehindType)
        {
            this.CodeBehindType = CodeBehindType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type CodeBehindType { get; private set; }
    }
}
