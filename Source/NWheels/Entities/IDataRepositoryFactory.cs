using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Conventions;
using System.Data;
using NWheels.Concurrency;

namespace NWheels.Entities
{
    public interface IDataRepositoryFactory : IAutoObjectFactory
    {
        IApplicationDataRepository NewUnitOfWork(
            IResourceConsumerScopeHandle consumerScope, 
            Type repositoryType, 
            bool autoCommit, 
            IsolationLevel? isolationLevel = null,
            string databaseName = null);

        Type GetDataRepositoryContract(Type entityContractType);
    }
}
