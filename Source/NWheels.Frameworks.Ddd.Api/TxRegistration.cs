using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Frameworks.Ddd
{
    public class TxRegistration
    {
        public TxRegistration(Type componentType)
        {
            this.ComponentType = componentType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type ComponentType { get; }
    }
}
