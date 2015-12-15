using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Concurrency;

namespace NWheels.Entities.Impl
{
    public class VoidDataRepositoryFactory : IDataRepositoryFactory
    {
        public IApplicationDataRepository NewUnitOfWork(
            IResourceConsumerScopeHandle consumerScope, 
            Type repositoryType, 
            bool autoCommit, 
            UnitOfWorkScopeOption? scopeOption = null,
            string databaseName = null)
        {
            throw new NotSupportedException();
        }

        public TService CreateService<TService>() where TService : class
        {
            throw new NotSupportedException();
        }

        public Type GetDataRepositoryContract(Type entityContractType)
        {
            throw new NotSupportedException();
        }

        public Type ServiceAncestorMarkerType
        {
            get { throw new NotSupportedException(); }
        }
    }
}
