using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.DataObjects;
using TT = Hapil.TypeTemplate;

namespace NWheels.Stacks.EntityFramework.Factories
{
    public abstract class PropertyConfigurationStrategy
    {
        protected PropertyConfigurationStrategy(ObjectFactoryContext factoryContext, ITypeMetadata metaType, IPropertyMetadata metaProperty)
        {
            this.FactoryContext = factoryContext;
            this.MetaType = metaType;
            this.MetaProperty = metaProperty;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract void OnWritingEfModelConfiguration(
            MethodWriterBase method, 
            Operand<DbModelBuilder> modelBuilder,
            Operand<ITypeMetadata> typeMetadata,
            Operand<ITypeMetadataCache> metadataCache,
            Operand<EntityTypeConfiguration<TT.TImpl>> typeConfig);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ObjectFactoryContext FactoryContext { get; private set; }
        public ITypeMetadata MetaType { get; private set; }
        public IPropertyMetadata MetaProperty { get; private set; }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected Type FindImpementationType(Type contractType)
        {
            return FactoryContext.Factory.FindDynamicType(contractType);
        }
    }
}
