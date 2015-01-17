using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Core.Logging;

namespace NWheels.Testing
{
    public class TestFramework : IFramework
    {
        private readonly IContainer _components;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TestFramework()
        {
            _components = BuildComponentContainer();

            this.PresetGuids = new Queue<Guid>();
            this.PresetRandomInt32 = new Queue<int>();
            this.PresetRandomInt64 = new Queue<long>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TInterface New<TInterface>() where TInterface : class
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Guid NewGuid()
        {
            return (PresetGuids.Count > 0 ? PresetGuids.Dequeue() : Guid.NewGuid());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public int NewRandomInt32()
        {
            return (PresetRandomInt32.Count > 0 ? PresetRandomInt32.Dequeue() : 123);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public long NewRandomInt64()
        {
            return (PresetRandomInt64.Count > 0 ? PresetRandomInt64.Dequeue() : 123);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Guid CorrelationId
        {
            get
            {
                return PresetCorrelationId.GetValueOrDefault(Guid.Empty);
            }
            set
            {
                PresetCorrelationId = value;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DateTime UtcNow
        {
            get
            {
                return PresetUtcNow.GetValueOrDefault(DateTime.UtcNow);
            }
            set
            {
                PresetUtcNow = value;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IComponentContext Components
        {
            get
            {
                return _components;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Queue<Guid> PresetGuids { get; private set; }
        public Queue<int> PresetRandomInt32 { get; private set; }
        public Queue<long> PresetRandomInt64 { get; private set; }
        public Guid? PresetCorrelationId { get; set; }
        public DateTime? PresetUtcNow { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private IContainer BuildComponentContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<ThreadLogRegistry>().SingleInstance();

            return builder.Build();
        }
    }
}
