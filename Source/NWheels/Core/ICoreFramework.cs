using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Entities;

namespace NWheels.Core
{
    public interface ICoreFramework
    {
        IApplicationDataRepository NewUnitOfWork(
            Type domainContextType, 
            bool autoCommit = true, 
            IsolationLevel? isolationLevel = null, 
            string databaseName = null);
        
        IApplicationDataRepository NewUnitOfWorkForEntity(
            Type entityContractType, 
            bool autoCommit = true, 
            IsolationLevel? isolationLevel = null, 
            string databaseName = null);
        
        IComponentContext Components { get; }
    }
}
