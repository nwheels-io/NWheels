using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hapil;
using Hapil.Members;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.DataObjects.Core.Factories;
using TT = Hapil.TypeTemplate;
using System.Reflection;

namespace NWheels.TypeModel.Core.Factories
{
    public class AutomaticPropertyStrategy : PropertyImplementationStrategy
    {
        public AutomaticPropertyStrategy(
            ObjectFactoryContext factoryContext, 
            ITypeMetadataCache metadataCache, 
            ITypeMetadata metaType, 
            IPropertyMetadata metaProperty)
            : base(factoryContext, metadataCache, metaType, metaProperty)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of PropertyImplementationStrategy

        protected override void OnImplementContractProperty(ImplementationClassWriter<TT.TInterface> writer)
        {
            writer.ImplementInterfaceVirtual<TT.TInterface>().Property(MetaProperty.ContractPropertyInfo).Implement(
                p => p.Get(m => {
                    base.ImplementedContractProperty = p.OwnerProperty.PropertyBuilder;
                    m.Return(p.BackingField);
                }),
                p => p.Set((m, value) => p.BackingField.Assign(value))
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementStorageProperty(ImplementationClassWriter<TT.TInterface> writer)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingInitializationConstructor(MethodWriterBase writer, Operand<IComponentContext> components, params IOperand[] args)
        {
            HelpInitializeDefaultValue(writer, components);
        }

        #endregion
    }
}
