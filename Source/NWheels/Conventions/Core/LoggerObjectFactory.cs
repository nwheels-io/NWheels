using System;
using Hapil;
using NWheels.Logging;
using NWheels.Logging.Core;

namespace NWheels.Conventions.Core
{
    internal class LoggerObjectFactory : ConventionObjectFactory, IAutoObjectFactory
    {
        private readonly Pipeline<IThreadLogAppender> _appenderPipeline;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LoggerObjectFactory(DynamicModule module, Pipeline<IThreadLogAppender> appenderPipeline)
            : base(module)
        {
            _appenderPipeline = appenderPipeline;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TService CreateService<TService>() where TService : class
        {
            return base.CreateInstanceOf<TService>().UsingConstructor<Pipeline<IThreadLogAppender>>(_appenderPipeline);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type ServiceAncestorMarkerType
        {
            get
            {
                return typeof(IApplicationEventLogger);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ConventionObjectFactory

        protected override IObjectFactoryConvention[] BuildConventionPipeline(ObjectFactoryContext context)
        {
            var staticStrings = new StaticStringsDecorator();

            return new IObjectFactoryConvention[] {
                new LoggerBaseTypeConvention(), 
                staticStrings,
                new ApplicationEventLoggerConvention(staticStrings)
            };
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class LoggerBaseTypeConvention : ImplementationConvention
        {
            public LoggerBaseTypeConvention()
                : base(Will.InspectDeclaration)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnInspectDeclaration(ObjectFactoryContext context)
            {
                if (context.TypeKey.PrimaryInterface.IsClass)
                {
                    context.BaseType = context.TypeKey.PrimaryInterface;
                }
            }
        }
    }
}
