using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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
            return BuildEnvironment<SingleActorWorkflow>(framework);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal static InstanceTestEnvironment<SingleEventAsyncWorkflow, IInstanceEntity> BuildSingleEventAsyncWorkflow(
            TestFramework framework,
            IInstanceEntity existingInstanceData = null)
        {
            return BuildEnvironment<SingleEventAsyncWorkflow>(framework, existingInstanceData);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal static InstanceTestEnvironment<MapReduceAsyncWorkflow, IInstanceEntity> BuildMapReduceAsyncWorkflow(
            TestFramework framework,
            IInstanceEntity existingInstanceData = null)
        {
            return BuildEnvironment<MapReduceAsyncWorkflow>(framework, existingInstanceData);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static InstanceTestEnvironment<TCodeBehind, IInstanceEntity> BuildEnvironment<TCodeBehind>(
            TestFramework framework,
            IInstanceEntity existingInstanceData = null) 
            where TCodeBehind : class, IWorkflowCodeBehind
        {
            InstanceTestEnvironment<TCodeBehind, IInstanceEntity> environment;

            if ( existingInstanceData == null )
            {
                using ( var repo = framework.NewUnitOfWork<IInstanceRepository>() )
                {
                    environment = new InstanceTestEnvironment<TCodeBehind, IInstanceEntity>(framework, repo.Workflows.New());
                }
            }
            else
            {
                environment = new InstanceTestEnvironment<TCodeBehind, IInstanceEntity>(framework, existingInstanceData);
            }

            return environment;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class SingleActorWorkflow : IInitializableWorkflowCodeBehind<IInstanceEntity>
        {
            public void OnBuildWorkflow(IWorkflowBuilder builder)
            {
                builder.AddActor("A1", 1, new EmptyActor(), new EmptyRouter());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void OnInitialize(IInstanceEntity initialData, IWorkflowInitializer initializer)
            {
                initializer.SetInitialWorkItem("ABC");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class SingleEventAsyncWorkflow : IInitializableWorkflowCodeBehind<IInstanceEntity>
        {
            public void OnBuildWorkflow(IWorkflowBuilder builder)
            {
                builder.AddActor("A1", 1, new EmptyActor(), new ConstantRouter(routeToActorName: "E1"), isInitial: true);
                builder.AddActor("E1", 0, new EventOneActor("K1"), new ConstantRouter(routeToActorName: "B1"));
                builder.AddActor("B1", 0, new EmptyActor(), new EmptyRouter());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void OnInitialize(IInstanceEntity initialData, IWorkflowInitializer initializer)
            {
                initializer.SetInitialWorkItem("ABC");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class MapReduceAsyncWorkflow : IInitializableWorkflowCodeBehind<IInstanceEntity>
        {
            public void OnBuildWorkflow(IWorkflowBuilder builder)
            {
                builder.AddActor("A1", 1, new EmptyActor(), new MapRouter("M1", "M2", "M3", "M4"), isInitial: true);
                builder.AddActor("M1", 0, new EventOneActor("KM1"), new ConstantRouter(routeToActorName: "R1"));
                builder.AddActor("M2", 0, new EventTwoActor("KM2"), new ConstantRouter(routeToActorName: "R1"));
                builder.AddActor("M3", 0, new EventOneActor("KM3"), new ConstantRouter(routeToActorName: "R1"));
                builder.AddActor("M4", 0, new EventTwoActor("KM4"), new ConstantRouter(routeToActorName: "R1"));
                builder.AddActor("R1", 1, new EmptyActor(), new ReduceRouter(branchCount: 4, routeToActorName: "Z1"));
                builder.AddActor("Z1", 1, new EmptyActor(), new EmptyRouter());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void OnInitialize(IInstanceEntity initialData, IWorkflowInitializer initializer)
            {
                initializer.SetInitialWorkItem("ABC");
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

        public class EventTwoActor : IWorkflowActor<string>
        {
            private readonly string _key;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public EventTwoActor(string key)
            {
                _key = key;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Execute(IWorkflowActorContext context, string workItem)
            {
                context.AwaitEvent<EventTwo, string>(_key, TimeSpan.FromMinutes(31));
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
                if ( context.HasActorWorkItem<string>() )
                {
                    context.EnqueueWorkItem(_routeToActorName, context.GetActorWorkItem<string>());
                }

                if ( context.HasReceivedEvent<EventOne>() )
                {
                    context.EnqueueWorkItem(_routeToActorName, context.GetReceivedEvent<EventOne>().Data);
                }

                if ( context.HasReceivedEvent<EventTwo>() )
                {
                    context.EnqueueWorkItem(_routeToActorName, context.GetReceivedEvent<EventTwo>().Data.ToString());
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Type GetCookieType()
            {
                return null;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class MapRouter : IWorkflowRouter
        {
            private readonly string[] _routeToActorNames;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public MapRouter(params string[] routeToActorNames)
            {
                _routeToActorNames = routeToActorNames;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Route(IWorkflowRouterContext context)
            {
                foreach ( var actorName in _routeToActorNames )
                {
                    context.EnqueueWorkItem(actorName, context.GetActorWorkItem<string>());
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Type GetCookieType()
            {
                return null;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ReduceRouter : IWorkflowRouter
        {
            private readonly int _branchCount;
            private readonly string _routeToActorName;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ReduceRouter(int branchCount, string routeToActorName)
            {
                _branchCount = branchCount;
                _routeToActorName = routeToActorName;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Route(IWorkflowRouterContext context)
            {
                var cookie = context.Cookie as MyCookie;

                if ( cookie == null )
                {
                    cookie = new MyCookie() {
                        Results = new List<string>()
                    };

                    context.Cookie = cookie;
                }

                cookie.Results.Add(context.GetActorWorkItem<string>());

                if ( cookie.Results.Count == _branchCount )
                {
                    context.EnqueueWorkItem(_routeToActorName, string.Join(";", cookie.Results));
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Type GetCookieType()
            {
                return typeof(MyCookie);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [DataContract]
            public class MyCookie
            {
                [DataMember]
                public List<string> Results { get; set; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class EmptyRouter : IWorkflowRouter
        {
            public void Route(IWorkflowRouterContext context)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Type GetCookieType()
            {
                return null;
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class EventTwo : WorkflowEventBase<string>
        {
            public EventTwo(string key, int data)
                : base(key)
            {
                this.Data = data;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public int Data { get; private set; }
        }
    }
}
