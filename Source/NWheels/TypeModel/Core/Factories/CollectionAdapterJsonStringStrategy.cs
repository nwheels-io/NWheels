using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using Newtonsoft.Json;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.Factories;
using NWheels.Extensions;
using TT = Hapil.TypeTemplate;

namespace NWheels.TypeModel.Core.Factories
{
    public class CollectionAdapterJsonStringStrategy : CollectionAdapterDualValueStrategy
    {
        public CollectionAdapterJsonStringStrategy(
            ObjectFactoryContext factoryContext, 
            ITypeMetadataCache metadataCache, 
            ITypeMetadata metaType, 
            IPropertyMetadata metaProperty)
            : base(factoryContext, metadataCache, metaType, metaProperty, storageType: typeof(string))
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of CollectionAdapterDualValueStrategy

        protected override void OnWritingConcreteCollectionToStorageConversion(
            MethodWriterBase method, 
            IOperand<TT.TConcreteCollection<TT.TConcrete>> concreteCollection, 
            MutableOperand<TT.TValue> storageValue)
        {
            storageValue.Assign(Static.Func(JsonConvert.SerializeObject, concreteCollection.CastTo<object>()).CastTo<TT.TValue>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingStorageToConcreteCollectionConversion(
            MethodWriterBase method,
            MutableOperand<TT.TConcreteCollection<TT.TConcrete>> concreteCollection, 
            IOperand<TT.TValue> storageValue)
        {
            concreteCollection.Assign(
                Static.Func(JsonConvert.DeserializeObject<TT.TConcreteCollection<TT.TConcrete>>, storageValue.CastTo<string>())
                .CastTo<TT.TConcreteCollection<TT.TConcrete>>()
            );
        }

        #endregion
    }
}
