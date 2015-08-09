using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.TypeModel.Core;

namespace NWheels.Entities.Core
{
    public interface IPersistableObject : IContainedIn<IDomainObject>
    {
        void SetContainerObject(IDomainObject container);
    }
}
