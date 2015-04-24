using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities;

namespace NWheels.Processing.Core
{
    public interface IWorkflowDataRepository : IApplicationDataRepository
    {
        IEntityRepository<IWorkflowInstanceEntity> Workflows { get; }
    }
}
