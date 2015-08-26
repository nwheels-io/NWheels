using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.DataObjects.Core.Factories;
using NWheels.TypeModel.Core.Factories;
using TT = Hapil.TypeTemplate;

namespace NWheels.Entities.Factories
{
    public class PresentationNestedObjectPropertyStrategy : PropertyImplementationStrategy
    {
        private readonly PresentationObjectFactoryContext _context;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PresentationNestedObjectPropertyStrategy(
            PresentationObjectFactoryContext context,
            IPropertyMetadata metaProperty)
            : base(context.BaseContext, context.MetadataCache, context.MetaType, metaProperty)
        {
            _context = context;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of PropertyImplementationStrategy

        protected override void OnImplementContractProperty(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
            writer.Property(MetaProperty.ContractPropertyInfo).Implement(
                p => MetaProperty.ContractPropertyInfo.CanRead ? 
                    p.Get(gw => {
                        this.ImplementedContractProperty = p.OwnerProperty.PropertyBuilder;
                        gw.Return(
                            _context.PresentationFactoryField.Func<TT.TProperty, TT.TProperty>(x => x.CreatePresentationObjectInstance<TT.TProperty>, 
                                _context.DomainObjectField.Prop<TT.TProperty>(MetaProperty.ContractPropertyInfo)
                            )
                        );
                    }) : 
                    null,
                p => MetaProperty.ContractPropertyInfo.CanWrite ? 
                    p.Set((sw, value) => {
                        this.ImplementedContractProperty = p.OwnerProperty.PropertyBuilder;
                        _context.DomainObjectField.Prop<TT.TProperty>(MetaProperty.ContractPropertyInfo).Assign(value);
                    }) :
                    null
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementStorageProperty(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override bool OnHasNestedObjects()
        {
            return true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingDeepListNestedObjects(MethodWriterBase writer, IOperand<HashSet<object>> nestedObjects)
        {
            var w = writer;

            Static.Void(RuntimeTypeModelHelpers.DeepListNestedObject,
                w.This<TT.TBase>().Prop<TT.TProperty>(this.ImplementedContractProperty),
                nestedObjects);
        }

        #endregion
    }
}
