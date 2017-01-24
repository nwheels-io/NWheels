using NWheels.Compilation.Mechanism.Factories;
using NWheels.Compilation.Policy;
using NWheels.Compilation.Policy.Relaxed;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.DataStructures
{
    public class EqualityComparerTypeFactory : 
        TypeFactoryBase<IRuntimeTypeFactoryArtifact>, 
        IEqualityComparerObjectFactory
    {
        public EqualityComparerTypeFactory(
            ITypeLibrary<IRuntimeTypeFactoryArtifact> library) : base(library)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ImplementEqualityComparer(Type comparedType)
        {
            var key = Library.CreateKey<Empty.KeyExtension>(primaryContract: comparedType);
            GetOrBuildTypeMember(key);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IEqualityComparer<T> IEqualityComparerObjectFactory.GetEqualityComparer<T>()
        {
            var key = Library.CreateKey<Empty.KeyExtension>(primaryContract: typeof(T));
            var product = Library.GetProduct(key);
            var instance = product.Artifact.GetOrCreateSingleton<IEqualityComparer<T>>(constructorIndex: 0);

            return instance;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        Type IEqualityComparerObjectFactory.GetEqualityComparerImplementation(Type comparedType)
        {
            var key = Library.CreateKey<Empty.KeyExtension>(primaryContract: comparedType);
            var product = Library.GetProduct(key);

            return product.Artifact.RunTimeType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DefinePipelineAndExtendFactoryContext(
            TypeKey<Empty.KeyExtension> key, 
            List<ITypeFactoryConvention> pipe, 
            out Empty.ContextExtension contextExtension)
        {
            pipe.Add(new EqualityComparerImplementationConvention());
            contextExtension = null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class EqualityComparerImplementationConvention : ConventionBase
        {
            protected override void Implement(
                ITypeFactoryContext<Empty.KeyExtension, Empty.ContextExtension> context, 
                TypeWriter writer)
            {
                
            }
        }
    }
}
