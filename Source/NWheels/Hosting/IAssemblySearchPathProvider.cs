using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Hosting
{
    public interface IAssemblySearchPathProvider
    {
        string[] GetAssemblySearchPaths(BootConfiguration node, BootConfiguration.ModuleConfig module);
    }
}
