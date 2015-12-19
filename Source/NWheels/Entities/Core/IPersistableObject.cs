using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects.Core;
using NWheels.TypeModel.Core;

namespace NWheels.Entities.Core
{
    public interface IPersistableObject : IObject //: IContainedIn<IDomainObject>
    {
        //void SetContainerObject(IDomainObject container);
        //void EnsureDomainObject();

        object[] ExportValues(IEntityRepository entityRepo);
        void ImportValues(IEntityRepository entityRepo, object[] values);
    }
}
