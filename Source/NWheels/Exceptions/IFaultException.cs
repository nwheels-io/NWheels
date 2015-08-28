using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Exceptions
{
    /// <summary>
    /// Applies to exceptions which provide fault codes and reason, as specified below.
    /// </summary>
    public interface IFaultException
    {
        /// <summary>
        /// Required. This is the top-level string id of the error.
        /// </summary>
        string FaultCode { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Optional. A more specific id of the error within the top-level fault code.
        /// </summary>
        string FaultSubCode { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Optional. Text message explaining the error condition. Should be a translation to current thread culture.
        /// </summary>
        string FaultReason { get; }
    }
}
