using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Tools.TestBoard.Services
{
    public interface IApplicationComponentInjector
    {
        void RegisterInjectedComponents(Autofac.ContainerBuilder containerBuilder);
    }
}
