using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Entities;
using NWheels.Extensions;
using NWheels.Logging;
using NWheels.Processing.Workflows;
using NWheels.Processing.Workflows.Core;
using NWheels.Testing;

namespace NWheels.UnitTests.Processing.Workflows.Impl
{
    public static class TestWorkflows
    {
        internal static InstanceTestEnvironment<SingleActorWorkflow, IInstanceEntity> BuildSingleActorWorkflow(TestFramework framework)
        {
            return BuildEnvironment<SingleActorWorkflow, IInstanceEntity>(
                framework, 
                newInstanceDataFactory: repo => repo.Workflows.New());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal static InstanceTestEnvironment<SingleEventAsyncWorkflow, IInstanceEntity> BuildSingleEventAsyncWorkflow(
            TestFramework framework,
            IInstanceEntity existingInstanceData = null)
        {
            return BuildEnvironment<SingleEventAsyncWorkflow, IInstanceEntity>(
                framework, 
                newInstanceDataFactory: repo => repo.Workflows.New(),
                existingInstanceData: existingInstanceData);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal static InstanceTestEnvironment<AsyncWorkflowWithLifecycleEvents, IInstanceEntity> BuildAsyncWorkflowWithLifecycleEvents(
            TestFramework framework,
            IInstanceEntity existingInstanceData = null)
        {
            framework.UpdateComponents(builder => builder.NWheelsFeatures().Logging().RegisterLogger<IWorkflowLifecycleLogger>());

            return BuildEnvironment<AsyncWorkflowWithLifecycleEvents, IInstanceEntity>(
                framework,
                newInstanceDataFactory: repo => repo.Workflows.New(),
                existingInstanceData: existingInstanceData);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal static InstanceTestEnvironment<MapReduceAsyncWorkflow, IInstanceEntity> BuildMapReduceAsyncWorkflow(
            TestFramework framework,
            IInstanceEntity existingInstanceData = null)
        {
            return BuildEnvironment<MapReduceAsyncWorkflow, IInstanceEntity>(
                framework,
                newInstanceDataFactory: repo => repo.Workflows.New(),
                existingInstanceData: existingInstanceData);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal static InstanceTestEnvironment<StateMachineWorkflow<OrderItemState, OrderItemTrigger, IOrderItemEntity>, IOrderItemEntity> 
            BuildStateMachineWorkflow(
                TestFramework framework,
                IOrderItemEntity existingInstanceData = null)
        {
            framework.UpdateComponents(builder => {
                builder.RegisterType<OrderItemWorkflow>().InstancePerDependency();
                builder.NWheelsFeatures().Processing().RegisterStateMachineWorkflow<
                    OrderItemState, 
                    OrderItemTrigger, 
                    OrderItemWorkflow, 
                    IInstanceRepository,
                    IOrderItemEntity>(repo => repo.OrderItems.AsQueryable());
                builder.NWheelsFeatures().Logging().RegisterLogger<IOrderItemLogger>();
            });

            return BuildEnvironment<StateMachineWorkflow<OrderItemState, OrderItemTrigger, IOrderItemEntity>, IOrderItemEntity>(
                framework, 
                newInstanceDataFactory: repo => repo.OrderItems.New(),
                existingInstanceData: existingInstanceData);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static InstanceTestEnvironment<TCodeBehind, TDataEntity> BuildEnvironment<TCodeBehind, TDataEntity>(
            TestFramework framework,
            Func<IInstanceRepository, TDataEntity> newInstanceDataFactory = null,
            TDataEntity existingInstanceData = null) 
            where TCodeBehind : class, IWorkflowCodeBehind
            where TDataEntity : class, IWorkflowInstanceEntity
        {
            InstanceTestEnvironment<TCodeBehind, TDataEntity> environment;

            if ( existingInstanceData == null && newInstanceDataFactory != null )
            {
                using ( var repo = framework.NewUnitOfWork<IInstanceRepository>() )
                {
                    environment = new InstanceTestEnvironment<TCodeBehind, TDataEntity>(framework, newInstanceDataFactory(repo));
                }
            }
            else
            {
                environment = new InstanceTestEnvironment<TCodeBehind, TDataEntity>(framework, existingInstanceData);
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

        public class AsyncWorkflowWithLifecycleEvents : 
            IInitializableWorkflowCodeBehind<IInstanceEntity>,
            ISuspendableWorkflowCodeBehind<IInstanceEntity>,
            IWorkflowCodeBehindLifecycle
        {
            private readonly IWorkflowLifecycleLogger _logger;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public AsyncWorkflowWithLifecycleEvents(IWorkflowLifecycleLogger logger)
            {
                _logger = logger;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void OnBuildWorkflow(IWorkflowBuilder builder)
            {
                _logger.OnBuildWorkflow();

                builder.AddActor("A1", 1, new EmptyActor(), new ConstantRouter(routeToActorName: "E1"), isInitial: true);
                builder.AddActor("E1", 0, new EventOneActor("K1"), new ConstantRouter(routeToActorName: "B1"));
                builder.AddActor("B1", 0, new EmptyActor(), new EmptyRouter());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void OnInitialize(IInstanceEntity initialData, IWorkflowInitializer initializer)
            {
                _logger.OnInitialize();

                initializer.SetInitialWorkItem("ABC");
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void OnStart()
            {
                _logger.OnStart();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void OnSuspend(IInstanceEntity dataToSave)
            {
                _logger.OnSuspend();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void OnResume(IInstanceEntity savedData)
            {
                _logger.OnResume();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void OnComplete()
            {
                _logger.OnComplete();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void OnFail(Exception error)
            {
                _logger.OnFail(error);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void OnFinalize()
            {
                _logger.OnFinalize();
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

        public interface IWorkflowLifecycleLogger : IApplicationEventLogger
        {
            [LogInfo]
            void OnBuildWorkflow();
            [LogInfo]
            void OnInitialize();
            [LogInfo]
            void OnStart();
            [LogInfo]
            void OnSuspend();
            [LogInfo]
            void OnResume();
            [LogInfo]
            void OnComplete();
            [LogInfo]
            void OnFail(Exception error);
            [LogInfo]
            void OnFinalize();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public enum OrderItemState
        {
            PaymentConfirmationPending,
            LockingStock,
            OrderingFromSupplier,
            WaitingForSupply,
            RequestingDelivery,
            DeliveryConfirmationPending,
            Delivered,
            Problematic,
            ProblemResolvedResuming
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public enum OrderItemTrigger
        {
            PaymentReceived,
            PaymentFailed,
            StockLocked,
            OutOfStock,
            StockSystemError,
            SupplierOrderAccepted,
            SupplierOrderDenied,
            SupplierOrderArrived,
            DeliveryRequestAccepted,
            DeliveryRequestDenied,
            DeliveryConfirmed,
            ProblemResolved // includes next state to continue from in the event data
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IOrderItemLogger : IApplicationEventLogger
        {
            [LogInfo]
            void PaymentConfirmationPending();
            [LogInfo]
            void PaymentReceived();
            [LogInfo]
            void PaymentFailed();
            [LogInfo]
            void LockingStock();
            [LogInfo]
            void StockLocked();
            [LogInfo]
            void OutOfStock();
            [LogError]
            void StockSystemError();
            [LogInfo]
            void RequestingDelivery();
            [LogInfo]
            void DeliveryRequestAccepted();
            [LogInfo]
            void DeliveryRequestDenied();
            [LogWarning]
            void DeliveryRequestTimedOut();
            [LogInfo]
            void DeliveryConfirmationPending();
            [LogInfo]
            void Delivered();
            [LogWarning]
            void DeliveryTimedOut();
            [LogWarning]
            void Problematic();
            [LogInfo]
            void OrderingFromSupplier();
            [LogInfo]
            void SupplierOrderDenied();
            [LogInfo]
            void WaitingForSupply();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class OrderItemWorkflow : IStateMachineCodeBehind<OrderItemState, OrderItemTrigger>
        {
            private readonly IOrderItemLogger _logger;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public OrderItemWorkflow(IOrderItemLogger logger)
            {
                _logger = logger;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void BuildStateMachine(IStateMachineBuilder<OrderItemState, OrderItemTrigger> machine)
            {
                machine.State(OrderItemState.PaymentConfirmationPending)
                    .SetAsInitial()
                    .OnEntered((sender, args) => _logger.PaymentConfirmationPending())
                    .OnTrigger(OrderItemTrigger.PaymentReceived).TransitionTo(OrderItemState.LockingStock)
                    .OnTrigger(OrderItemTrigger.PaymentFailed).TransitionTo(OrderItemState.Problematic, (sender, args) => _logger.PaymentFailed());

                machine.State(OrderItemState.LockingStock)
                    .OnEntered((sender, args) => _logger.LockingStock())
                    .OnTrigger(OrderItemTrigger.StockLocked).TransitionTo(OrderItemState.RequestingDelivery)
                    .OnTrigger(OrderItemTrigger.OutOfStock).TransitionTo(OrderItemState.OrderingFromSupplier)
                    .OnTrigger(OrderItemTrigger.StockSystemError).TransitionTo(OrderItemState.Problematic, (sender, args) => _logger.StockSystemError());

                machine.State(OrderItemState.RequestingDelivery)
                    .OnEntered((sender, args) => _logger.RequestingDelivery())
                    .OnTrigger(OrderItemTrigger.DeliveryRequestAccepted).TransitionTo(OrderItemState.DeliveryConfirmationPending)
                    .OnTrigger(OrderItemTrigger.DeliveryRequestDenied).TransitionTo(OrderItemState.Problematic, (sender, args) => _logger.DeliveryRequestDenied());

                machine.State(OrderItemState.DeliveryConfirmationPending)
                    .OnEntered((sender, args) => _logger.DeliveryConfirmationPending())
                    .OnTrigger(OrderItemTrigger.DeliveryConfirmed).TransitionTo(OrderItemState.Delivered);

                machine.State(OrderItemState.OrderingFromSupplier)
                    .OnEntered((sender, args) => _logger.OrderingFromSupplier())
                    .OnTrigger(OrderItemTrigger.SupplierOrderAccepted).TransitionTo(OrderItemState.WaitingForSupply)
                    .OnTrigger(OrderItemTrigger.SupplierOrderDenied).TransitionTo(OrderItemState.Problematic, (sender, args) => _logger.SupplierOrderDenied());

                machine.State(OrderItemState.WaitingForSupply)
                    .OnEntered((sender, args) => _logger.WaitingForSupply())
                    .OnTrigger(OrderItemTrigger.SupplierOrderArrived).TransitionTo(OrderItemState.LockingStock);

                machine.State(OrderItemState.Delivered)
                    .OnEntered((sender, args) => _logger.Delivered());
                
                machine.State(OrderItemState.Problematic);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [EntityContract]
        public interface IInstanceEntity : IWorkflowInstanceEntity
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [EntityContract]
        public interface IOrderItemEntity : IStateMachineInstanceEntity<OrderItemState>
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IInstanceRepository : IApplicationDataRepository
        {
            IEntityRepository<IInstanceEntity> Workflows { get; }
            IEntityRepository<IOrderItemEntity> OrderItems { get; }
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
