using System.Collections.Generic;
using Hapil;
using Hapil.Applied.Conventions;
using NWheels.Conventions.Core;
using NWheels.Logging.Factories;

namespace NWheels.Hosting.Factories
{
    public class ComponentAspectFactory : ConventionObjectFactory
    {
        public ComponentAspectFactory(DynamicModule module)
            : base(module)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ConventionObjectFactory

        protected override IObjectFactoryConvention[] BuildConventionPipeline(ObjectFactoryContext context)
        {
            var staticStrings = new StaticStringsDecorator();

            return new IObjectFactoryConvention[] {
                staticStrings,
                new CallLoggingAspectConvention(staticStrings), 
            };
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------


    }
}
