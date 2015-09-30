using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using NWheels.UI.Uidl;

namespace NWheels.Stacks.NancyFx
{
    public interface IWebModuleContext
    {
        UidlDocument Uidl { get; }
        UidlApplication Application { get; }
        string SkinName { get; }
        string SkinSubFolderName { get; }
        string BaseSubFolderName { get; }
        IRootPathProvider PathProvider { get; }
        IReadOnlyDictionary<string, object> ApiServicesByContractName { get; }
        IReadOnlyDictionary<string, WebApiDispatcherBase> ApiDispatchersByContractName { get; }
    }
}
