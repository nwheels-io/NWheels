using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities;
using NWheels.Modules.Auth;

namespace NWheels.Samples.BloggingPlatform.Domain
{
    public interface IReplyEntity : IEntityPartId<long>, IEntityPartAudit
    {
        string Contents { get; set; }
        ReplyStatus Status { get; set; }
    }
}
