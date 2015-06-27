using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Uidl;

namespace NWheels.Stacks.NancyFx
{
    public interface IWebModuleContext
    {
        UidlDocument Uidl { get; }
        UidlApplication Application { get; }
        IReadOnlyDictionary<string, object> ApiServicesByContractName { get; }
        IReadOnlyDictionary<string, WebApiDispatcherBase> ApiDispatchersByContractName { get; }
    }
}
