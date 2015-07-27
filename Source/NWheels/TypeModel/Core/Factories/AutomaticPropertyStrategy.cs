using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.DataObjects.Core.Factories;
using TT = Hapil.TypeTemplate;

namespace NWheels.TypeModel.Core.Factories
{
    public class AutomaticPropertyStrategy : PropertyImplementationStrategy
    {
        public AutomaticPropertyStrategy(ObjectFactoryContext factoryContext, ITypeMetadataCache metadataCache, ITypeMetadata metaType, IPropertyMetadata metaProperty)
            : base(factoryContext, metadataCache, metaType, metaProperty)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of PropertyImplementationStrategy

        protected override bool OnShouldApply(IPropertyMetadata metaProperty)
        {
            return (
                metaProperty.Kind == PropertyKind.Scalar && 
                !metaProperty.ClrType.IsCollectionType() && 
                metaProperty.ContractPropertyInfo.CanRead && 
                metaProperty.ContractPropertyInfo.CanWrite);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override PropertyImplementationStrategy OnClone(IPropertyMetadata metaProperty)
        {
            return new AutomaticPropertyStrategy(base.FactoryContext, base.MetadataCache, base.MetaType, metaProperty);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementContractProperty(ClassWriterBase writer)
        {
            writer.ImplementInterfaceVirtual(MetaType.ContractType).Property(MetaProperty.ContractPropertyInfo).ImplementAutomatic();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementStorageProperty(ClassWriterBase writer)
        {
        }

        #endregion
    }
}
