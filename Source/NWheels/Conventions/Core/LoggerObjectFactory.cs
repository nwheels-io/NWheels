using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using NWheels.Conventions;
using NWheels.Core.Logging;
using NWheels.Logging;

namespace NWheels.Core.Conventions
{
    internal class LoggerObjectFactory : ConventionObjectFactory, IAutoObjectFactory
    {
        private readonly IThreadLogAppender _appender;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LoggerObjectFactory(DynamicModule module, IThreadLogAppender appender)
            : base(module, new ApplicationEventLoggerConvention())
        {
            _appender = appender;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TService CreateService<TService>() where TService : class
        {
            return base.CreateInstanceOf<TService>().UsingConstructor<IThreadLogAppender>(_appender);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type ServiceAncestorMarkerType
        {
            get
            {
                return typeof(IApplicationEventLogger);
            }
        }
    }
}
