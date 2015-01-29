using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using NWheels.Core.Hosting;

namespace NWheels.Hosts.Service
{
    public class WindowsService : ServiceBase
    {
        private NodeHost _nodeHost;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public WindowsService()
        {
            base.ServiceName = Program.HostConfig.ApplicationName + "." + Program.HostConfig.NodeName + ".";
            base.AutoLog = true;
            base.CanStop = true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnStart(string[] args)
        {
            _nodeHost = new NodeHost(Program.HostConfig, RegisterHostComponents);
            _nodeHost.LoadAndActivate();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnStop()
        {
            _nodeHost.DeactivateAndUnload();
            _nodeHost = null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private void RegisterHostComponents(ContainerBuilder builder)
        {
            builder.RegisterModule<NWheels.Puzzle.Nlog.ModuleLoader>();
        }
    }
}
