using NWheels.Compilation.Mechanism.Factories;
using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Platform.Rest;

namespace NWheels.Frameworks.Ddd.RestApi
{
    public class TxResourceHandlerTypeFactory : TypeFactoryBase<IRuntimeTypeFactoryArtifact>, ITxResourceHandlerObjectFactory
    {
        public TxResourceHandlerTypeFactory(ITypeLibrary<IRuntimeTypeFactoryArtifact> mechanism) 
            : base(mechanism)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void EnsureResourceHandlersCompiled(Type txType)
        {
            var key = new TypeKey(this.GetType(), txType);
            base.GetOrBuildTypeMember(ref key);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        ITxResourceHandlerList ITxResourceHandlerObjectFactory.CreateResourceHandlerList(Type txType)
        {
            var key = new TypeKey(this.GetType(), txType);
            var product = base.Library.GetProduct(ref key);
            var constructor = product.Artifact.For<ITxResourceHandlerList>().Constructor();
            var handlerList = constructor.NewInstance();
            return handlerList;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DefinePipelineAndExtendFactoryContext(
            TypeKey key, 
            List<ITypeFactoryConvention> pipeline, 
            out Empty.ContextExtension contextExtension)
        {
            pipeline.Add(new TxResourceHandlerListConvention());
            contextExtension = null;
        }
    }
}
