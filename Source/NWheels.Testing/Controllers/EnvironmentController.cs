using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Hosting;
using NWheels.Logging.Core;

namespace NWheels.Testing.Controllers
{
    public class EnvironmentController : CompositeControllerBase<NodeController>
    {
        private readonly BootConfiguration _bootConfig;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EnvironmentController(ApplicationController parentController, BootConfiguration bootConfig, Action<Autofac.ContainerBuilder> onInjectComponents)
            : base(parentController)
        {
            _bootConfig = bootConfig;
            base.AddSubController(new NodeController(this, bootConfig, onInjectComponents));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string DisplayName
        {
            get
            {
                return string.Format("{0}:{1}", _bootConfig.EnvironmentName, _bootConfig.EnvironmentType);
            }
        }
    }
}
