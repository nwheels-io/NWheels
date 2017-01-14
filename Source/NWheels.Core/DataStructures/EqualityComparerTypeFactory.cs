using NWheels.Compilation.Mechanism.Factories;
using NWheels.Compilation.Policy;
using NWheels.Compilation.Policy.Relaxed;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.DataStructures
{
    public class EqualityComparerTypeFactory : TypeFactoryBase<IRuntimeTypeFactoryArtifact>, IEqualityComparerTypeFactory
    {
        public EqualityComparerTypeFactory(
            ITypeFactoryMechanism<IRuntimeTypeFactoryArtifact> mechanism) : base(mechanism)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEqualityComparer<T> GetEqualityComparer<T>()
        {
            var key = Mechanism.CreateKey<Empty.KeyExtension>(primaryContract: typeof(T));
            var product = Mechanism.GetOrBuildProduct(key);

            var instance = product.Artifact.GetInstance<IEqualityComparer<T>>(singleton: true, constructorIndex: 0);
            return instance;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type GetEqualityComparerImplementation(Type comparedType)
        {
            var key = Mechanism.CreateKey<Empty.KeyExtension>(primaryContract: comparedType);
            var product = Mechanism.GetOrBuildProduct(key);

            return product.Artifact.RunTimeType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnBuildingNewProduct(
            ITypeKey<Empty.KeyExtension> key, 
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
