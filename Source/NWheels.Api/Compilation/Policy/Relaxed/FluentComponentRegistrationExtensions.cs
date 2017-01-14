#if false

using NWheels.Compilation.Mechanism.Factories;
using NWheels.Injection.Mechanism;
using NWheels.Injection.Policy;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Api.Compilation.Policy
{
    public static class FluentComponentRegistrationExtensions
    {
        public static CompilationComponents ToCompilation(this FluentComponentContribution fluent)
        {
            return new CompilationComponents(fluent.ContainerBuilder);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class CompilationComponents
        {
            public static readonly string FactoryTypeMetadataKey = "CompilationComponents.FactoryType";

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private readonly IComponentContainerBuilder _builder;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public CompilationComponents(IComponentContainerBuilder builder)
            {
                _builder = builder;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public FluentComponentRegistration<TConvention> TypeFactoryConvention<TConvention>(Type factoryType)
                where TConvention : class, ITypeFactoryConvention
            {
                return _builder
                    .RegisterType<TConvention>()
                    .As<ITypeFactoryConvention>()
                    .WithMetadata(FactoryTypeMetadataKey, factoryType);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public FluentComponentRegistration<object> TypeFactoryConvention(Type conventionType, Type factoryType)
            {
                return _builder
                    .RegisterType(conventionType)
                    .As<ITypeFactoryConvention>()
                    .WithMetadata(FactoryTypeMetadataKey, factoryType);
            }
        }
    }
}

#endif