using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace NWheels.Processing.Core
{
    public abstract class WorkflowTypeRegistration
    {
        public abstract IQueryable<IWorkflowInstanceEntity> GetDataEntityQuery(IComponentContext components);
        public abstract Type CodeBehindType { get; }
        public abstract Type DataEntityType { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class WorkflowTypeRegistration<TCodeBehind, TDataRepository, TDataEntity> : WorkflowTypeRegistration
        where TCodeBehind : class, IWorkflowCodeBehind
        where TDataEntity : class, IWorkflowInstanceEntity
    {
        private readonly Func<TDataRepository, IQueryable<TDataEntity>> _entityRepositoryFunc;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public WorkflowTypeRegistration(Func<TDataRepository, IQueryable<TDataEntity>> entityRepositoryFunc)
        {
            _entityRepositoryFunc = entityRepositoryFunc;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IQueryable<IWorkflowInstanceEntity> GetDataEntityQuery(IComponentContext components)
        {
            var dataRepository = components.Resolve<TDataRepository>();
            var dataEntityQueryable = _entityRepositoryFunc(dataRepository);

            return dataEntityQueryable;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override Type CodeBehindType
        {
            get { return typeof(TCodeBehind); }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override Type DataEntityType
        {
            get { return typeof(TDataEntity); }
        }
    }
}
