using System;
using NWheels.Api.Uidl.Specification;

namespace NWheels.Api.Uidl.Toolbox
{
    public class ManagementConsole<TViewModel>
    {
        public Configurator Configure()
        {
            return null;
        }

        public UIPartContainer<TViewModel> ActiveContent { get; set; }

        public class Configurator
        {
            public Configurator Navigation(object anonymous)
            {
                return this;
            }
            
            public object MenuItem(object icon = null, UidlBehavior action = null)
            {
                return this;
            }
        }
    }
}
