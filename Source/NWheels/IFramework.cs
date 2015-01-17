using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels
{
    public interface IFramework
    {
        TInterface New<TInterface>() where TInterface : class;
        Guid NewGuid();
        int NewRandomInt32();
        long NewRandomInt64();
        Guid CorrelationId { get; }
        DateTime UtcNow { get; }
    }
}
