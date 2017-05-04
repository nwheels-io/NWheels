using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Platform.Rest
{
    public abstract class ResourceProtocolHandlerBase : IResourceProtocolHandler
    {
        protected ResourceProtocolHandlerBase(string name)
        {
            this.Name = name;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Name { get; }
    }
}
