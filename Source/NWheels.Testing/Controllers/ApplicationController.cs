using System;
using NWheels.Hosting;
using NWheels.Logging.Core;

namespace NWheels.Testing.Controllers
{
    public class ApplicationController : CompositeControllerBase<EnvironmentController>
    {
        private readonly BootConfiguration _bootConfig;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ApplicationController(IPlainLog plainLog, BootConfiguration bootConfig)
            : base(plainLog)
        {
            _bootConfig = bootConfig;
            base.AddSubController(new EnvironmentController(this, bootConfig));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string DisplayName
        {
            get
            {
                return _bootConfig.ApplicationName;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public BootConfiguration BootConfig
        {
            get { return _bootConfig; }
        }
    }
}
