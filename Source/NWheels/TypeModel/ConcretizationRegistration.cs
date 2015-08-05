using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.DataObjects
{
    public class ConcretizationRegistration
    {
        public ConcretizationRegistration(Type generalContract, Type concreteContract, Type domainObject)
        {
            this.GeneralContract = generalContract;
            this.ConcreteContract = concreteContract;
            this.DomainObject = domainObject;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type GeneralContract { get; private set; }
        public Type ConcreteContract { get; private set; }
        public Type DomainObject { get; private set; }
    }
}
