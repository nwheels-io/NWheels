using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Uidl;

namespace NWheels.UI.Core
{
    public interface IUidlApplicationContext
    {
        UidlDocument Uidl { get; }
        UidlApplication Application { get; }
        ApplicationEntityService EntityService { get; }
        string SkinName { get; }
    }
}
