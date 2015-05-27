using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities;
using NWheels.Processing.Workflows;
using NWheels.Processing.Workflows.Core;
using NWheels.Testing;

namespace NWheels.UnitTests.Processing.Workflows.Impl
{
    public static class TestWorkflows
    {
        internal static InstanceTestEnvironment<SingleActorWorkflow, IInstanceEntity> BuildSingleActorWorkflow(TestFramework framework)
        {
            using ( var repo = framework.NewUnitOfWork<IInstanceRepository>() )
            {
                return new InstanceTestEnvironment<SingleActorWorkflow, IInstanceEntity>(framework, repo.Workflows.New());
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class SingleActorWorkflow : IWorkflowCodeBehind
        {
            public void OnBuildWorkflow(IWorkflowBuilder builder)
            {
                builder.AddActor("A1", 1, new EmptyActor(), new EmptyRouter());
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IInstanceEntity : IWorkflowInstanceEntity
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IInstanceRepository : IApplicationDataRepository
        {
            IEntityRepository<IInstanceEntity> Workflows { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class EmptyActor : IWorkflowActor<string>
        {
            public void Execute(IWorkflowActorContext context, string workItem)
            {
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ConstantRouter : IWorkflowRouter
        {
            private readonly string _routeToActorName;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ConstantRouter(string routeToActorName)
            {
                _routeToActorName = routeToActorName;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Route(IWorkflowRouterContext context)
            {
                context.EnqueueWorkItem(_routeToActorName, context.GetActorWorkItem<string>());
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class EmptyRouter : IWorkflowRouter
        {
            public void Route(IWorkflowRouterContext context)
            {
            }
        }
    }
}
