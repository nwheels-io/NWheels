using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Hosting;
using NWheels.Logging.Core;

namespace NWheels.Testing.Controllers
{
    public class NodeController : CompositeControllerBase<NodeInstanceController>
    {
        private readonly BootConfiguration _bootConfig;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NodeController(EnvironmentController parentController, BootConfiguration bootConfig)
            : base(parentController)
        {
            _bootConfig = bootConfig;
            base.AddSubController(new NodeInstanceController(this, bootConfig, instanceId: "1"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string DisplayName
        {
            get
            {
                return _bootConfig.NodeName;
            }
        }
    }
}
