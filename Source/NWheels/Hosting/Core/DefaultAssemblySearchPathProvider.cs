using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Utilities;

namespace NWheels.Hosting.Core
{
    public class DefaultAssemblySearchPathProvider : IAssemblySearchPathProvider
    {
        public virtual string[] GetAssemblySearchPaths(BootConfiguration node, BootConfiguration.ModuleConfig module)
        {
            var coreBinPath = PathUtility.HostBinPath(module.Assembly);
            var appBinPath = Path.Combine(node.LoadedFromDirectory, module.Assembly);

            return new[] {
                coreBinPath,
                appBinPath
            };
        }
    }
}
