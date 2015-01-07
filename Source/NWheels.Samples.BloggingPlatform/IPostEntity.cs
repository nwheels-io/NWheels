using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities;
using NWheels.Modules.Auth;

namespace NWheels.Samples.BloggingPlatform
{
    public interface IPostEntity : IAbstractContentEntity
    {
        IQueryable<IReplyEntity> Replies { get; }
    }
}
