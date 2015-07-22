using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Hosting.Core;

namespace NWheels.Testing.Controllers
{
    public class ControllerRequestFailedException : Exception
    {
        public ControllerRequestFailedException(string message)
            : base(message)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal ControllerRequestFailedException(ControllerBase controller, NodeTrigger request)
            : base(string.Format("Controller request [{0}] has failed. See log for details.", request))
        {
        }
    }
}
