using System;
using Hapil;
using NWheels.Logging;
using NWheels.Logging.Core;

namespace NWheels.Conventions.Core
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
