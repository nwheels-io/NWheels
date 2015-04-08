using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.DataObjects
{
    public class ConcretizationRegistration
    {
        public ConcretizationRegistration(Type generalContract, Type concreteContract)
        {
            this.GeneralContract = generalContract;
            this.ConcreteContract = concreteContract;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type GeneralContract { get; private set; }
        public Type ConcreteContract { get; private set; }
    }
}
