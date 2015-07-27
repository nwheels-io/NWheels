using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.DataObjects
{
    public class MixinRegistration
    {
        public MixinRegistration(Type targetContract, Type mixinContract)
        {
            this.TargetContract = targetContract;
            this.MixinContract = mixinContract;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type TargetContract { get; private set; }
        public Type MixinContract { get; private set; }
    }
}
