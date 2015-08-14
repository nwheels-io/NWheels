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
        IApplicationDataRepository NewUnitOfWorkForEntity(Type entityContractType, bool autoCommit = true, IsolationLevel? isolationLevel = null);
        IComponentContext Components { get; }
    }
}
