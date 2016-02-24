using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using NWheels.Entities;
using NWheels.Logging;
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
        T NewDomainObject<T>(IComponentContext externalComponents) where T : class;

        Thread CreateThread(Action threadCode, Func<ILogActivity> threadLogFactory = null, ThreadTaskType? taskType = null, string description = null);
        void RunThreadCode(Action threadCode, Func<ILogActivity> threadLogFactory = null, ThreadTaskType? taskType = null, string description = null);

        IComponentContext Components { get; }
        IReadOnlyThreadLog CurrentThreadLog { get; }
    }
}
