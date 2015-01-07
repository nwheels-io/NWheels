using System.Collections.Generic;
using NWheels.Entities;
using NWheels.Modules.Auth;

namespace NWheels.Samples.BloggingPlatform
{
    public interface IAbstractContentEntity : IEntityPartId<int>, IEntityPartActiveStatus, IEntityPartAudit
    {
        string Contents { get; set; }
        ISet<ITagEntity> Tags { get; }
    }
}
