using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Hosting;
using NWheels.Hosting.Core;

namespace NWheels.Testing.Controllers
{
    public class NodeInstanceController : ControllerBase
    {
        private readonly BootConfiguration _bootConfig;
        private readonly string _instanceId;
        private NodeHost _nodeHost;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
 
        public NodeInstanceController(
            NodeController parentController,
            BootConfiguration bootConfig, 
            string instanceId)
            : base(parentController)
        {
            _bootConfig = bootConfig;
            _instanceId = instanceId;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string DisplayName
        {
            get { return string.Format("Instance {0}", _instanceId); }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnLoad()
        {
            _nodeHost = new NodeHost(_bootConfig, OnInjectingComponents);
            _nodeHost.Load();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnActivate()
        {
            _nodeHost.Activate();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnDeactivate()
        {
            _nodeHost.Deactivate();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnUnload()
        {
            _nodeHost.Unload();
            _nodeHost = null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnInjectingComponents(ContainerBuilder containerBuilder)
        {
            base.OnInjectingComponents(containerBuilder);
        }
    }
}
