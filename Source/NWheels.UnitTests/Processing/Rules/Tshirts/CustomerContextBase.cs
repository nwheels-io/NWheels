using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UnitTests.Processing.Rules.Tshirts
{
    /// <summary>
    /// This is an example of a base class for rule system context
    /// </summary>
    public abstract class CustomerContextBase
    {
        protected CustomerContextBase(ICustomerEntity customer)
        {
            this.Customer = customer;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ICustomerEntity Customer { get; private set; }
    }
}
