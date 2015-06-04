using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Writers;

namespace NWheels.UI.Impl
{
    public class UIObjectFactory : ConventionObjectFactory
    {
        public UIObjectFactory(DynamicModule module)
            : base(module)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IObjectFactoryConvention[] BuildConventionPipeline(ObjectFactoryContext context)
        {
            return base.BuildConventionPipeline(context);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class UIObjectConvention : ImplementationConvention
        {
            public UIObjectConvention()
                : base(Will.ImplementPrimaryInterface)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnImplementPrimaryInterface(ImplementationClassWriter<TypeTemplate.TInterface> writer)
            {
                
            }
        }
    }
}
