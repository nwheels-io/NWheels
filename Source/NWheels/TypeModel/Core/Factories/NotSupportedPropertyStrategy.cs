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
    public class NotSupportedPropertyStrategy : PropertyImplementationStrategy
    {
        public NotSupportedPropertyStrategy(
            PropertyImplementationStrategyMap ownerMap,
            ObjectFactoryContext factoryContext, 
            ITypeMetadataCache metadataCache, 
            ITypeMetadata metaType, 
            IPropertyMetadata metaProperty)
            : base(ownerMap, factoryContext, metadataCache, metaType, metaProperty)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of PropertyImplementationStrategy

        protected override void OnImplementContractProperty(ImplementationClassWriter<TT.TInterface> writer)
        {
            var canRead = MetaProperty.ContractPropertyInfo.CanRead;
            var canWrite = MetaProperty.ContractPropertyInfo.CanWrite;

            writer.Property(MetaProperty.ContractPropertyInfo).Implement(
                getter: p =>
                    canRead
                    ? p.Get(m => m.Throw<NotSupportedException>("Property is not supported"))
                    : null,
                setter: p =>
                    canWrite
                    ? p.Set((m, value) => m.Throw<NotSupportedException>("Property is not supported"))
                    : null
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementStorageProperty(ImplementationClassWriter<TT.TInterface> writer)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingInitializationConstructor(MethodWriterBase writer, Operand<IComponentContext> components, params IOperand[] args)
        {
        }

        #endregion
    }
}
