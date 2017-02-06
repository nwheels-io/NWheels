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
            var key = new TypeKey(this.GetType(), comparedType);
            GetOrBuildTypeMember(ref key);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IEqualityComparer<T> IEqualityComparerObjectFactory.GetEqualityComparer<T>()
        {
            var key = new TypeKey(factoryType: this.GetType(), primaryContract: typeof(T));
            var product = Library.GetProduct(ref key);
            var instance = product.Artifact.For<IEqualityComparer<T>>().GetOrCreateSingleton();

            return instance;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        Type IEqualityComparerObjectFactory.GetEqualityComparerImplementation(Type comparedType)
        {
            var key = new TypeKey(factoryType: this.GetType(), primaryContract: comparedType);
            var product = Library.GetProduct(ref key);

            return product.Artifact.RunTimeType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DefinePipelineAndExtendFactoryContext(
            TypeKey key, 
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
                ITypeFactoryContext<Empty.ContextExtension> context, 
                TypeWriter writer)
            {
            }
        }
    }
}
