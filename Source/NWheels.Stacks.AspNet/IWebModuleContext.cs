using System.Collections.Generic;
using NWheels.UI;
using NWheels.UI.Uidl;

namespace NWheels.Stacks.AspNet
{
    public interface IWebModuleContext
    {
        UidlDocument Uidl { get; }
        UidlApplication Application { get; }
        ApplicationEntityService EntityService { get; }
        string SkinName { get; }
        string SkinSubFolderName { get; }
        string BaseSubFolderName { get; }
        string ContentRootPath { get; }
        IWebApplicationLogger Logger { get; }
    }
}
