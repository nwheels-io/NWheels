using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace NWheels.Testing.Controllers
{
    public class ComponentInjectionEventArgs : EventArgs
    {
        public ComponentInjectionEventArgs(ContainerBuilder containerBilder)
        {
            this.ContainerBuilder = containerBilder;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Autofac.ContainerBuilder ContainerBuilder { get; private set; }
    }
}
