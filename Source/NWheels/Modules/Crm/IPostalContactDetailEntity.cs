using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Modules.Geo;

namespace NWheels.Modules.Crm
{
    public interface IPostalContactDetailEntity : IAbstractContactDetailEntity
    {
        IEntityPartPostalAddress Address { get; set; }
        CustomerPostalAddressType AddressType { get; set; }
    }
}
