using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hapil;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.DataObjects.Core.Factories;

namespace NWheels.Stacks.MongoDb.Factories
{
    public class LazyLoadCollectionByForeignKeyStrategy : PropertyImplementationStrategy
    {
        public LazyLoadCollectionByForeignKeyStrategy(ObjectFactoryContext factoryContext, ITypeMetadataCache metadataCache, ITypeMetadata metaType, IPropertyMetadata metaProperty)
            : base(factoryContext, metadataCache, metaType, metaProperty)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of PropertyImplementationStrategy

        protected override void OnImplementContractProperty(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementStorageProperty(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
