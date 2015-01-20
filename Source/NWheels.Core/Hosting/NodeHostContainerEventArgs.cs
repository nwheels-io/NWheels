using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace NWheels.Core.Hosting
{
    public class NodeHostContainerEventArgs : EventArgs
    {
        public NodeHostContainerEventArgs(NodeHost nodeHost, ContainerBuilder builder)
        {
            this.NodeHost = nodeHost;
            this.Builder = builder;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NodeHost NodeHost { get; private set; }
        public ContainerBuilder Builder { get; private set; }
    }
}
