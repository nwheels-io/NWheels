using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Decorators;
using Hapil.Members;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Extensions;
using TT = Hapil.TypeTemplate;

namespace NWheels.Stacks.MongoDb.Factories
{
    public class MongoLazyLoadProxyFactory : ConventionObjectFactory
    {
        private readonly ITypeMetadataCache _metadataCache;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MongoLazyLoadProxyFactory(DynamicModule module, ITypeMetadataCache metadataCache)
            : base(module)
        {
            _metadataCache = metadataCache;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type GetLazyLoadProxyType(Type contractType, Type implementationType)
        {
            var metaType = _metadataCache.GetTypeMetadata(contractType);
            var key = new ProxyTypeKey(metaType);
            var typeEntry = GetOrBuildType(key);
            return typeEntry.DynamicType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ConventionObjectFactory

        protected override IObjectFactoryConvention[] BuildConventionPipeline(ObjectFactoryContext context)
        {
            var metaType = _metadataCache.GetTypeMetadata(context.TypeKey.PrimaryInterface);

            return new IObjectFactoryConvention[] { 
                new ProxyConvention(metaType), 
            };
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ObjectFactoryBase

        public override TypeKey CreateTypeKey(Type contractType, params Type[] secondaryInterfaceTypes)
        {
            var metaType = _metadataCache.GetTypeMetadata(contractType);
            return new ProxyTypeKey(metaType);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ProxyTypeKey : TypeKey
        {
            public ProxyTypeKey(ITypeMetadata metaType)
                : base(primaryInterface: metaType.ContractType, baseType: GetProxyBaseType(metaType))
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            private static Type GetProxyBaseType(ITypeMetadata metaType)
            {
                var closedBaseType = typeof(MongoLazyLoadProxyBase<,>).MakeGenericType(metaType.ContractType, metaType.EntityIdProperty.ClrType);
                return closedBaseType;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ProxyConvention : ImplementationConvention
        {
            private readonly ITypeMetadata _metaType;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ProxyConvention(ITypeMetadata metaType)
                : base(Will.ImplementBaseClass | Will.ImplementPrimaryInterface)
            {
                _metaType = metaType;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of ImplementationConvention

            protected override void OnImplementBaseClass(ImplementationClassWriter<TT.TBase> writer)
            {
                using ( TT.CreateScope<TT.TKey>(_metaType.EntityIdProperty.ClrType) )
                {
                    writer.Constructor<TT.TKey>((cw, id) => {
                        cw.Base(id);
                    });
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnImplementPrimaryInterface(ImplementationClassWriter<TT.TInterface> writer)
            {
                ImplementEntityIdProperty(writer);
                ImplementEntityDataProperties(writer);
                ImplementEntityMethods(writer);
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementEntityDataProperties(ImplementationClassWriter<TT.TInterface> writer)
            {
                writer.AllProperties(@where: p => IsEntityProperty(p) && !IsEntityIdProperty(p)).Implement(
                    p => p.Get(gw => {
                        WritePropertyGetterDelegation(p, gw);
                    }),
                    p => p.Set((sw, value) => {
                        WritePropertySetterDelegation(p, sw, value);
                    })
                );
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementEntityIdProperty(ImplementationClassWriter<TT.TInterface> writer)
            {
                writer.Property(_metaType.EntityIdProperty.ContractPropertyInfo).Implement(
                    p => p.Get(gw => {
                        gw.Return(
                            gw.This<MongoLazyLoadProxyBase<TT.TInterface, TT.TProperty>>().Func<TT.TProperty>(x => x.MongoLazyLoadProxyBaseGetId)
                        );
                    }),
                    p => p.Set((sw, value) => {
                        WritePropertySetterDelegation(p, sw, value);
                    })
                );
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementEntityMethods(ImplementationClassWriter<TT.TInterface> writer)
            {
                writer.AllMethods(IsEntityMethod).Throw<NotSupportedException>();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            private void WritePropertyGetterDelegation(TemplatePropertyWriter p, FunctionMethodWriter<TT.TProperty> gw)
            {
                if ( p.OwnerProperty.PropertyDeclaration.CanRead )
                {
                    gw.Return(
                        gw.This<MongoLazyLoadProxyBase<TT.TInterface, TT.TProperty>>()
                        .Func<TT.TInterface>(x => x.MongoLazyLoadProxyBaseGetReal)
                        .Prop<TT.TProperty>(p.OwnerProperty.PropertyDeclaration));
                }
                else
                {
                    gw.Throw<NotSupportedException>("This property cannot be read by contract.");
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void WritePropertySetterDelegation(TemplatePropertyWriter p, VoidMethodWriter sw, IOperand<TT.TProperty> value)
            {
                if ( p.OwnerProperty.PropertyDeclaration.CanWrite )
                {
                    sw.This<MongoLazyLoadProxyBase<TT.TInterface, TT.TProperty>>()
                        .Func<TT.TInterface>(x => x.MongoLazyLoadProxyBaseGetReal)
                        .Prop<TT.TProperty>(p.OwnerProperty.PropertyDeclaration)
                        .Assign(value);
                }
                else
                {
                    sw.Throw<NotSupportedException>("This property cannot be written by contract.");
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            private bool IsEntityIdProperty(PropertyInfo declaration)
            {
                var result = (declaration == _metaType.EntityIdProperty.ContractPropertyInfo);
                return result;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool IsEntityProperty(PropertyInfo declaration)
            {
                IPropertyMetadata metaProperty;
                var result = _metaType.TryGetPropertyByDeclaration(declaration, out metaProperty);
                return result;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            private bool IsEntityMethod(MethodInfo method)
            {
                var result = (method.DeclaringType != null && (
                    method.DeclaringType.IsEntityContract() || 
                    method.DeclaringType.IsEntityPartContract() ||
                    typeof(IObject).IsAssignableFrom(method.DeclaringType) ||
                    method.DeclaringType == typeof(IActiveRecord)));
                
                return result;
            }
        }
    }
}
