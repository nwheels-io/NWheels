using NWheels.Compilation.Mechanism.Factories;
using NWheels.Compilation.Mechanism.Syntax.Members;
using NWheels.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Policy.Relaxed
{
    public abstract class ConventionBase<TContextExtension> : ITypeFactoryConvention
    {
        protected ConventionBase()
        {
            this.ConventionName = this.GetType().Name.TrimSuffix("Convention");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected ConventionBase(string conventionName)
        {
            this.ConventionName = conventionName;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

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
            context.Type.Namespace = this.GetType().Namespace;

            if (context.Key.PrimaryContract != null)
            {
                context.Type.Name = $"{ConventionName}_Of_{context.Key.PrimaryContract.FullName}";
            }
            else
            {
                context.Type.Name = $"{ConventionName}_Of_UnknownComponent";
            }

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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected Exception NewValidationException(TypeMember type, string message, Exception inner = null)
        {
            return NewValidationException(type, null, message, inner);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected Exception NewValidationException(TypeMember type, AbstractMember member, string message, Exception inner = null)
        {
            throw new NotImplementedException();
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected string ConventionName { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class ConventionBase : ConventionBase<Empty.ContextExtension>
    {
    }
}
