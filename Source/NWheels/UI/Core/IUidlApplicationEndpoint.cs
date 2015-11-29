using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Endpoints.Core;
using NWheels.UI.Uidl;

namespace NWheels.UI.Core
{
    public interface IUidlApplicationEndpoint : IEndpoint
    {
        UidlApplication UidlApplication { get; }
    }
}
