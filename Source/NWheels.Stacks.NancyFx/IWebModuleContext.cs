using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using NWheels.UI;
using NWheels.UI.Impl;
using NWheels.UI.Uidl;

namespace NWheels.Stacks.NancyFx
{
    public interface IWebModuleContext
    {
        UidlDocument Uidl { get; }
        UidlApplication Application { get; }
        ApplicationEntityService EntityService { get; }
        string SkinName { get; }
        string SkinSubFolderName { get; }
        string BaseSubFolderName { get; }
        IRootPathProvider PathProvider { get; }
        IWebApplicationLogger Logger { get; }
        IReadOnlyDictionary<string, object> ApiServicesByContractName { get; }
        IReadOnlyDictionary<string, WebApiDispatcherBase> ApiDispatchersByContractName { get; }
    }
}
