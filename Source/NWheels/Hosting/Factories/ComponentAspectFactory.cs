using System.Collections.Generic;
using Hapil;
using Hapil.Applied.Conventions;
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
            return new IObjectFactoryConvention[] {
                  
            };
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------


    }
}
