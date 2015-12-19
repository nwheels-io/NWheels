using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.TypeModel.Core;
using NWheels.DataObjects.Core;

namespace NWheels.Entities.Core
{
    public interface IDomainObject : IObject //, IContain<IPersistableObject>
    {
        void Validate();
        void BeforeCommit();
        void AfterCommit();
        object[] ExportValues(IEntityRepository entityRepo);
        void ImportValues(IEntityRepository entityRepo, object[] values);
        void InitializeValues(bool idManuallyAssigned);
        void SetLazyLoader(IPersistableObjectLazyLoader lazyLoader);
        EntityState State { get; }
        object TemporaryKey { get; set; }
    }
}
