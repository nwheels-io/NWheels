using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Core.Logging;
using NWheels.Extensions;

namespace NWheels.Core
{
    internal class RealFramework : IFramework
    {
        private readonly IComponentContext _components;
        private readonly IThreadLogAnchor _threadLogAnchor;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public RealFramework(IComponentContext components, IThreadLogAnchor threadLogAnchor)
        {
            _components = components;
            _threadLogAnchor = threadLogAnchor;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TInterface New<TInterface>() where TInterface : class
        {
            return _components.ResolveAuto<TInterface>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Guid NewGuid()
        {
            return Guid.NewGuid();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public int NewRandomInt32()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public long NewRandomInt64()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Guid CorrelationId
        {
            get
            {
                var currentThreadLog = _threadLogAnchor.CurrentThreadLog;
                return (currentThreadLog != null ? currentThreadLog.CorrelationId : Guid.Empty);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DateTime UtcNow
        {
            get
            {
                return DateTime.UtcNow;
            }
        }
    }
}
