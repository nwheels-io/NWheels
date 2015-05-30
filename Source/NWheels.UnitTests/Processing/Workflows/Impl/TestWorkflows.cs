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

        internal static InstanceTestEnvironment<SingleEventAsyncWorkflow, IInstanceEntity> BuildSingleEventAsyncWorkflow(
            TestFramework framework, 
            IInstanceEntity existingInstanceData = null)
        {
            if ( existingInstanceData == null )
            {
                using ( var repo = framework.NewUnitOfWork<IInstanceRepository>() )
                {
                    return new InstanceTestEnvironment<SingleEventAsyncWorkflow, IInstanceEntity>(framework, repo.Workflows.New());
                }
            }
            else
            {
                return new InstanceTestEnvironment<SingleEventAsyncWorkflow, IInstanceEntity>(framework, existingInstanceData);
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

        public class SingleEventAsyncWorkflow : IWorkflowCodeBehind
        {
            public void OnBuildWorkflow(IWorkflowBuilder builder)
            {
                builder.AddActor("A1", 1, new EmptyActor(), new ConstantRouter(routeToActorName: "E1"), isInitial: true);
                builder.AddActor("E1", 0, new EventOneActor("K1"), new ConstantRouter(routeToActorName: "B1"));
                builder.AddActor("B1", 0, new EmptyActor(), new EmptyRouter());
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [EntityContract]
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

        public class EventOneActor : IWorkflowActor<string>
        {
            private readonly string _key;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public EventOneActor(string key)
            {
                _key = key;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Execute(IWorkflowActorContext context, string workItem)
            {
                context.AwaitEvent<EventOne, string>(_key, TimeSpan.FromMinutes(31));
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
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class EventOne : WorkflowEventBase<string>
        {
            public EventOne(string key, string data)
                : base(key)
            {
                this.Data = data;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string Data { get; private set; }
        }
    }
}
