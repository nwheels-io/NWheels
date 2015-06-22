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
        string ReasonText { get; }
        object ReasonCode { get; }
    }
}
