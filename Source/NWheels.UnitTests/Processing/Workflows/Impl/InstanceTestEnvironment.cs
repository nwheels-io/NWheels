using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using NWheels.Extensions;
using NWheels.Processing.Workflows;
using NWheels.Processing.Workflows.Core;
using NWheels.Processing.Workflows.Impl;
using NWheels.Testing;

namespace NWheels.UnitTests.Processing.Workflows.Impl
{
    internal class InstanceTestEnvironment<TCodeBehind, TDataEntity>
        where TDataEntity : class, IWorkflowInstanceEntity
        where TCodeBehind : class, IWorkflowCodeBehind
    {
        public InstanceTestEnvironment(TestFramework framework, TDataEntity instanceData)
        {
            this.Framework = framework;
            this.Components = Framework.Components;

            var updater = new ContainerBuilder();
            updater.NWheelsFeatures().Logging().RegisterLogger<IWorkflowEngineLogger>();
            updater.RegisterType<TCodeBehind>().PreserveExistingDefaults();
            updater.Update(this.Components.ComponentRegistry);

            this.InstanceData = instanceData;
            this.InstanceContext = new TestWorkflowInstanceContext(this);
            this.CodeBehindAdapter = new WorkflowCodeBehindAdapter<TCodeBehind, TDataEntity>();
            this.AwaitEventRequests = new List<AwaitEventRequest>();
            
            this.InstanceUnderTest = new WorkflowInstance(this.InstanceContext);
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public TestFramework Framework { get; private set; }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public IComponentContext Components { get; private set; }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public TDataEntity InstanceData { get; private set; }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public TestWorkflowInstanceContext InstanceContext { get; private set; }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public WorkflowCodeBehindAdapter<TCodeBehind, TDataEntity> CodeBehindAdapter { get; private set; }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public WorkflowInstance InstanceUnderTest { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public List<AwaitEventRequest> AwaitEventRequests { get; private set; }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestWorkflowInstanceContext : IWorkflowInstanceContext
        {
            private readonly InstanceTestEnvironment<TCodeBehind, TDataEntity> _environment;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TestWorkflowInstanceContext(InstanceTestEnvironment<TCodeBehind, TDataEntity> environment)
            {
                _environment = environment;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void AwaitEvent(Type eventType, object eventKey, TimeSpan timeout)
            {
                _environment.AwaitEventRequests.Add(new AwaitEventRequest(eventType, eventKey, timeout));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Autofac.IComponentContext Components
            {
                get { return _environment.Components; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IFramework Framework
            {
                get { return _environment.Framework; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public WorkflowCodeBehindAdapter CodeBehindAdapter
            {
                get { return _environment.CodeBehindAdapter; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public NWheels.Processing.Workflows.IWorkflowInstanceEntity InstanceData
            {
                get { return _environment.InstanceData; }
            }
        }
    }
}
