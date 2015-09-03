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

        public ConcretizationRegistration Merge(ConcretizationRegistration other)
        {
            if ( other == null )
            {
                throw new ArgumentNullException("other");
            }

            if ( this.GeneralContract != other.GeneralContract )
            {
                throw new ArgumentException("GeneralContract mismatch.");
            }

            var mergedConcreteContract = MostConcreteType(this.ConcreteContract, other.ConcreteContract);
            var mergedDomainObject = MostConcreteType(this.DomainObject, other.DomainObject);

            return new ConcretizationRegistration(this.GeneralContract, mergedConcreteContract, mergedDomainObject);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type GeneralContract { get; private set; }
        public Type ConcreteContract { get; private set; }
        public Type DomainObject { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Type MostConcreteType(Type type1, Type type2)
        {
            if ( type1 != null && type2 != null )
            {
                return (type1.IsAssignableFrom(type2) ? type2 : type1);
            }
            else
            {
                return type1 ?? type2;
            }
        }
    }
}
