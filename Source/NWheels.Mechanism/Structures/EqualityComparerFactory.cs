using NWheels.Compilation.Policy;
using NWheels.Compilation.Mechanism.Factories;
using NWheels.Structures;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace NWheels.Mechanism.Structures
{
    public class EqualityComparerFactory : TypeFactoryBase<bool, object>, IEqualityComparerFactory
    {
        private ImmutableDictionary<Type, object> _comparersByComparedType;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EqualityComparerFactory(ITypeFactoryMechanism mechanism) : base(mechanism)
        {
            _comparersByComparedType = ImmutableDictionary<Type, object>.Empty;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void EnsureComparersImplemented(Type[] comparedTypes)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEqualityComparer<T> GetComparer<T>()
        {
            return (IEqualityComparer<T>)_comparersByComparedType[typeof(T)];
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type GetComparerImplementation<T>()
        {
            var key = Mechanism.CreateKey<object>(typeof(T), Type.EmptyTypes, extension: null);
            var product = Mechanism.GetOrBuildProduct(key);
            return product.RunTimeType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnBuildingNewProduct(ITypeKey<bool> key, List<ITypeFactoryConvention> pipe, out object contextExtension)
        {
            contextExtension = null;
        }
    }
}
