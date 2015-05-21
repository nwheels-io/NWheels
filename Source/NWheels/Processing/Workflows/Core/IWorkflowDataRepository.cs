using NWheels.Entities;

namespace NWheels.Processing.Workflows.Core
{
    public interface IWorkflowDataRepository : IApplicationDataRepository
    {
        IEntityRepository<IWorkflowInstanceEntity> Workflows { get; }
    }
}
