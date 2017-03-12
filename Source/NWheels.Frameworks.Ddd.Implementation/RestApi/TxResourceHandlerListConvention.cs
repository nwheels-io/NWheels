using NWheels.Compilation.Mechanism.Factories;
using NWheels.Compilation.Policy.Relaxed;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Frameworks.Ddd.RestApi
{
    public class TxResourceHandlerListConvention : ConventionBase<Empty.ContextExtension>
    {
        protected override void Declare(ITypeFactoryContext<Empty.ContextExtension> context)
        {
            //TODO: provide a more convenient API for this
            context.Type.Namespace = this.GetType().Namespace;
            context.Type.Name = $"TxResourceHandlerList_Of_{context.Key.PrimaryContract.FullName}";
            context.Type.BaseType = typeof(ITxResourceHandlerList);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void Implement(ITypeFactoryContext<Empty.ContextExtension> context, TypeWriter writer)
        {

        }
    }
}
