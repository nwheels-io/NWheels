using System;
using NWheels.Api.Uidl;
using NWheels.Api.Uidl.Specification;

namespace NWheels.Api.Uidl.Toolbox
{
    public class UIPartContainer<TViewModel>
    {
        public UidlBehavior NavigateTo(object uiPart)
        {
            throw new NotImplementedException();
        }

        public Configurator Configure()
        {
            return null;
        }

        public class Configurator
        {
        }
    }
}
