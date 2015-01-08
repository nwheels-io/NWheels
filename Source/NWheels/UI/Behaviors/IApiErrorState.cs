using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Behaviors
{
    public interface IApiErrorState
    {
        string FaultCode { get; set; }
        string FaultSubCode { get; set; }
        string Message { get; set; }
    }
}
