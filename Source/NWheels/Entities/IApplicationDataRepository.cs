using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities.Core;

namespace NWheels.Entities
{
    public interface IApplicationDataRepository : IUnitOfWork
    {
        void InvokeGenericOperation(Type entityContractType, IDataRepositoryCallback callback);
        Type[] GetEntityTypesInRepository();
        Type[] GetEntityContractsInRepository();
        IEntityRepository[] GetEntityRepositories();
        IEntityRepository GetEntityRepository(Type entityContractType);
        bool TryGetEntityRepository(Type entityContractType, out IEntityRepository entityRepository);
    }
}
