using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Entities
{
    public interface IDomainContextPopulator
    {
        void Populate(IApplicationDataRepository context);
        Type ContextType { get; }
    }
}
