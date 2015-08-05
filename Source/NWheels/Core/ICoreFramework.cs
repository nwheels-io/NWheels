using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace NWheels.Core
{
    internal interface ICoreFramework
    {
        IComponentContext Components { get; }
    }
}
