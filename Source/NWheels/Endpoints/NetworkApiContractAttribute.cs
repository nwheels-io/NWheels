using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Endpoints
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class NetworkApiContractAttribute : Attribute
    {
    }
}
