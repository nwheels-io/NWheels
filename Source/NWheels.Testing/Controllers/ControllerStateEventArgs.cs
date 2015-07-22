using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Hosting;

namespace NWheels.Testing.Controllers
{
    public class ControllerStateEventArgs : EventArgs
    {
        public ControllerStateEventArgs(ControllerBase controller, NodeState state)
        {
            this.Controller = controller;
            this.State = state;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ControllerBase Controller { get; private set; }
        public NodeState State { get; private set; }
    }
}
