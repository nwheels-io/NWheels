using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Entities;
using NWheels.Entities.Core;

namespace NWheels.Core
{
    public interface ICoreFramework
    {
        IApplicationDataRepository NewUnitOfWork(
            Type domainContextType, 
            bool autoCommit = true, 
            UnitOfWorkScopeOption? scopeOption = null, 
            string databaseName = null);
        
        IApplicationDataRepository NewUnitOfWorkForEntity(
            Type entityContractType, 
            bool autoCommit = true, 
            UnitOfWorkScopeOption? scopeOption = null, 
            string databaseName = null);

        IDomainObject NewDomainObject(Type contractType);

        IComponentContext Components { get; }
    }
}
