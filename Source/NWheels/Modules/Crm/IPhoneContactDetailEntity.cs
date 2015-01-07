using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Modules.Crm
{
    public interface IPhoneContactDetailEntity : IAbstractContactDetailEntity
    {
        string PhoneNumber { get; set; }
        CustomerPhoneType PhoneType { get; set; }
    }
}
