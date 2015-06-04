using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Entities.Impl
{
    public class VoidDataRepositoryFactory : IDataRepositoryFactory
    {
        public IApplicationDataRepository NewUnitOfWork(Type repositoryType, bool autoCommit, System.Data.IsolationLevel? isolationLevel = null)
        {
            throw new NotSupportedException();
        }

        public TRepository NewUnitOfWork<TRepository>(bool autoCommit, System.Data.IsolationLevel? isolationLevel = null) where TRepository : class, IApplicationDataRepository
        {
            throw new NotSupportedException();
        }

        public TService CreateService<TService>() where TService : class
        {
            throw new NotSupportedException();
        }

        public Type ServiceAncestorMarkerType
        {
            get { throw new NotSupportedException(); }
        }
    }
}
