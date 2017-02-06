using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Members
{
    public enum MethodParameterModifier
    {
        None,
        Ref,
        Out,

        //currently not supported:
        //OutVar // out parameter with inline declaration of local variable ('declaration expression')
    }
}
