using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Modules.Crm
{
    public interface IEmailContactDetailEntity : IAbstractContactDetailEntity
    {
        string EmailAddress { get; set; }
        CustomerEmailAddressType EmailType { get; set; }
    }
}
