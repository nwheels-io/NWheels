using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace NWheels.DataObjects.Core
{
    public interface IHaveDependencies
    {
        void InjectDependencies(IComponentContext components);
    }
}
