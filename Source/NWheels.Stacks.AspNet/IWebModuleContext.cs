using System.Collections.Generic;
using NWheels.UI;
using NWheels.UI.Core;
using NWheels.UI.Uidl;

namespace NWheels.Stacks.AspNet
{
    public interface IWebModuleContext : IUidlApplicationContext
    {
        string SkinSubFolderName { get; }
        string BaseSubFolderName { get; }
        string ContentRootPath { get; }
        IWebApplicationLogger Logger { get; }
    }
}
