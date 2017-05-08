using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Platform.Rest
{
    public abstract class ResourceProtocolHandlerBase : IResourceProtocolHandler
    {
        protected ResourceProtocolHandlerBase(string name, Type protocolInterface)
        {
            this.Name = name;
            this.ProtocolInterface = protocolInterface;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Name { get; }
        public Type ProtocolInterface { get; }
    }
}
