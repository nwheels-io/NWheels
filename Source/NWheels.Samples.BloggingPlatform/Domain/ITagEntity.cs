using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities;

namespace NWheels.Samples.BloggingPlatform.Domain
{
    public interface ITagEntity : IEntityPartId<int>
    {
        string Name { get; set; }
        string Description { get; set; }
        IQueryable<IAbstractContentEntity> RelatedContents { get; }
    }
}
