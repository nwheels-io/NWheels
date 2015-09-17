using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    /// <summary>
    /// Encasulates error information for a failed promise.
    /// </summary>
    public interface IPromiseFailureInfo
    {
        string FaultCode { get; }
        string FaultSubCode { get; }
        string FaultReason { get; }
    }
}
