using NWheels.Compilation.Mechanism.Factories;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Policy.Relaxed
{
    public abstract class ConventionBase<TKeyExtension, TContextExtension> : ITypeFactoryConvention
        where TKeyExtension : ITypeKeyExtension, new()
    {
        bool ITypeFactoryConvention.ShouldApply(ITypeFactoryContext context)
        {
            return this.ShouldApply((ITypeFactoryContext<TKeyExtension, TContextExtension>)context);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void ITypeFactoryConvention.Validate(ITypeFactoryContext context)
        {
            this.Validate((ITypeFactoryContext<TKeyExtension, TContextExtension>)context);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void ITypeFactoryConvention.Declare(ITypeFactoryContext context)
        {
            this.Declare((ITypeFactoryContext<TKeyExtension, TContextExtension>)context);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void ITypeFactoryConvention.Implement(ITypeFactoryContext context)
        {
            //var writer = new TypeWriter()
            this.Implement((ITypeFactoryContext<TKeyExtension, TContextExtension>)context, null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual bool ShouldApply(ITypeFactoryContext<TKeyExtension, TContextExtension> context)
        {
            return true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void Validate(ITypeFactoryContext<TKeyExtension, TContextExtension> context)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void Declare(ITypeFactoryContext<TKeyExtension, TContextExtension> context)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void Implement(ITypeFactoryContext<TKeyExtension, TContextExtension> context, TypeWriter writer)
        {
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class ConventionBase<TContextExtension> : ConventionBase<Empty.KeyExtension, TContextExtension>
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class ConventionBase : ConventionBase<Empty.KeyExtension, Empty.ContextExtension>
    {
    }
}
