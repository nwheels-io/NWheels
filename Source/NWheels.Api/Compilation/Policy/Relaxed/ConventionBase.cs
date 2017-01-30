using NWheels.Compilation.Mechanism.Factories;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Policy.Relaxed
{
    public abstract class ConventionBase<TContextExtension> : ITypeFactoryConvention
    {
        bool ITypeFactoryConvention.ShouldApply(ITypeFactoryContext context)
        {
            return this.ShouldApply((ITypeFactoryContext<TContextExtension>)context);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void ITypeFactoryConvention.Validate(ITypeFactoryContext context)
        {
            this.Validate((ITypeFactoryContext<TContextExtension>)context);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void ITypeFactoryConvention.Declare(ITypeFactoryContext context)
        {
            this.Declare((ITypeFactoryContext<TContextExtension>)context);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void ITypeFactoryConvention.Implement(ITypeFactoryContext context)
        {
            //var writer = new TypeWriter()
            this.Implement((ITypeFactoryContext<TContextExtension>)context, null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual bool ShouldApply(ITypeFactoryContext<TContextExtension> context)
        {
            return true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void Validate(ITypeFactoryContext<TContextExtension> context)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void Declare(ITypeFactoryContext<TContextExtension> context)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void Implement(ITypeFactoryContext<TContextExtension> context, TypeWriter writer)
        {
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class ConventionBase : ConventionBase<Empty.ContextExtension>
    {
    }
}
