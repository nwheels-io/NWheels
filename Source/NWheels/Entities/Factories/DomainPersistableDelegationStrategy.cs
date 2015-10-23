using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Members;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.DataObjects.Core.Factories;
using NWheels.Exceptions;
using NWheels.TypeModel.Core;
using TT = Hapil.TypeTemplate;

namespace NWheels.Entities.Factories
{
    public class DomainPersistableDelegationStrategy : PropertyImplementationStrategy
    {
        private readonly DomainObjectFactoryContext _context;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DomainPersistableDelegationStrategy(
            PropertyImplementationStrategyMap ownerMap,
            DomainObjectFactoryContext context,
            IPropertyMetadata metaProperty)
            : base(ownerMap, context.BaseContext, context.MetadataCache, context.MetaType, metaProperty)
        {
            _context = context;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of PropertyImplementationStrategy

        protected override void OnImplementContractProperty(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
            var baseProperty = _context.GetBasePropertyToImplement(MetaProperty);
            ClassWriterBase effectiveClassWriter = (
                baseProperty.DeclaringType != null && baseProperty.DeclaringType.IsInterface 
                ? (ClassWriterBase)writer.ImplementInterfaceVirtual(baseProperty.DeclaringType) 
                : (ClassWriterBase)writer);

            using ( TT.CreateScope<TT.TInterface>(MetaType.ContractType) )
            {
                writer.Property(baseProperty).Implement(
                    p => baseProperty.GetMethod != null ? 
                        p.Get(gw => {
                            this.ImplementedContractProperty = p.OwnerProperty.PropertyBuilder;

                            if ( MetaProperty.ContractPropertyInfo.CanRead )
                            {
                                gw.Return(_context.PersistableObjectField.Prop<TT.TProperty>(MetaProperty.ContractPropertyInfo));
                            }
                            else
                            {
                                var readerMethodInfo = _context.PersistableObjectMembers.Methods.First(m => m.Name == GetReadAccessorMethodName(MetaProperty));
                                gw.Return(_context.PersistableObjectField.Func<TT.TProperty>(readerMethodInfo));
                            }
                        }) : 
                        null,
                    p => baseProperty.SetMethod != null ? 
                        p.Set((sw, value) => {
                            this.ImplementedContractProperty = p.OwnerProperty.PropertyBuilder;

                            if (MetaProperty.ContractPropertyInfo.CanWrite)
                            {
                                _context.PersistableObjectField.Prop<TT.TProperty>(MetaProperty.ContractPropertyInfo).Assign(value);
                            }
                            else
                            {
                                var writerMethodInfo = _context.PersistableObjectMembers.Methods.First(m => m.Name == GetWriteAccessorMethodName(MetaProperty));
                                _context.PersistableObjectField.Void(writerMethodInfo, value);
                            }
                        }) :
                        null
                );
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementStorageProperty(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingValidation(MethodWriterBase writer)
        {
            var w = writer;

            if ( MetaProperty.ClrType == typeof(string) )
            {
                if ( MetaProperty.Validation.IsRequired && MetaProperty.Validation.IsEmptyAllowed )
                {
                    w.If(w.This<TT.TBase>().Prop<string>(this.ImplementedContractProperty).IsNull()).Then(() => {
                        w.Throw<EntityValidationException>(MetaType.Name + "." + MetaProperty.Name + " is mandatory.");        
                    });
                }
                else if ( MetaProperty.Validation.IsRequired && !MetaProperty.Validation.IsEmptyAllowed )
                {
                    w.If(Static.Func(string.IsNullOrEmpty, w.This<TT.TBase>().Prop<string>(this.ImplementedContractProperty))).Then(() => {
                        w.Throw<EntityValidationException>(MetaType.Name + "." + MetaProperty.Name + " is mandatory and cannot be empty.");
                    });
                }

                if ( MetaProperty.Validation.MinLength.HasValue )
                {
                    w.If(
                        (!w.This<TT.TBase>().Prop<string>(this.ImplementedContractProperty).IsNull()) && 
                        w.This<TT.TBase>().Prop<string>(this.ImplementedContractProperty).Prop<int>(s => s.Length) < w.Const(MetaProperty.Validation.MinLength.Value)
                    )
                    .Then(() => {
                        w.Throw<EntityValidationException>(MetaType.Name + "." + MetaProperty.Name + " must be at least " + MetaProperty.Validation.MinLength.Value + " characters length.");
                    });
                }

                if ( MetaProperty.Validation.MaxLength.HasValue )
                {
                    w.If(
                        (!w.This<TT.TBase>().Prop<string>(this.ImplementedContractProperty).IsNull()) &&
                        w.This<TT.TBase>().Prop<string>(this.ImplementedContractProperty).Prop<int>(s => s.Length) > w.Const(MetaProperty.Validation.MaxLength.Value)
                    )
                    .Then(() => {
                        w.Throw<EntityValidationException>(MetaType.Name + "." + MetaProperty.Name + " length must not exceed " + MetaProperty.Validation.MaxLength.Value + " characters.");
                    });
                }
            }
        }

        #endregion
    }
}
