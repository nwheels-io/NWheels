using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.Conventions;
using NWheels.Entities.Core;
using NWheels.TypeModel;

namespace NWheels.DataObjects.Core.Conventions
{
    public class MutationMetadataConvention : IMetadataConvention
    {
        private readonly IComponentContext _components;
        private readonly Pipeline<IMetadataMutation> _mutations;
        private TypeMetadataCache _cache;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MutationMetadataConvention(IComponentContext components, Pipeline<IMetadataMutation> mutations)
        {
            _components = components;
            _mutations = mutations;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IMetadataConvention

        public void InjectCache(TypeMetadataCache cache)
        {
            _cache = cache;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Preview(TypeMetadataBuilder type)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Apply(TypeMetadataBuilder type)
        {
            foreach (var mutation in _mutations)
            {
                if (mutation.EntityContract.IsAssignableFrom(type.ContractType))
                {
                    mutation.Apply(_components, _cache, type);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Finalize(TypeMetadataBuilder type)
        {
        }

        #endregion
    }
}
