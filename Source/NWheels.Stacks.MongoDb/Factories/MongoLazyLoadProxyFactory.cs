using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Decorators;
using Hapil.Members;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.Entities;
using NWheels.Entities.Core;
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
            var key = new ProxyTypeKey(contractType, implementationType);
            var typeEntry = GetOrBuildType(key);
            return typeEntry.DynamicType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ConventionObjectFactory

        protected override IObjectFactoryConvention[] BuildConventionPipeline(ObjectFactoryContext context)
        {
            var metaType = _metadataCache.GetTypeMetadata(context.TypeKey.PrimaryInterface);

            var proxyConvention = new ProxyConvention(metaType);
            var entityObjectConvention = new ProxyEntityObjectConvention(proxyConvention);
            var lazyLoadConvention = new LazyLoadConvention(proxyConvention, entityObjectConvention);

            return new IObjectFactoryConvention[] { 
                proxyConvention, 
                entityObjectConvention,
                lazyLoadConvention
            };
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ObjectFactoryBase

        public override TypeKey CreateTypeKey(Type contractType, params Type[] secondaryInterfaceTypes)
        {
            return new ProxyTypeKey(contractType, null);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ProxyTypeKey : TypeKey
        {
            public ProxyTypeKey(Type contractType, Type implementationType)
                : base(primaryInterface: contractType, secondaryInterfaces: new[] { typeof(IMongoLazyLoadProxy) })
            {
                this.ImplementationType = implementationType;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Type ImplementationType { get; private set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ProxyConvention : ImplementationConvention
        {
            private readonly ITypeMetadata _metaType;
            private readonly IPropertyMetadata _idMetaProperty;
            private ProxyTypeKey _proxyTypeKey;
            private Field<TypeTemplate.TKey> _idField;
            private Field<TypeTemplate.TInterface> _objectField;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ProxyConvention(ITypeMetadata metaType)
                : base(Will.InspectDeclaration | Will.ImplementPrimaryInterface)
            {
                _metaType = metaType;
                _idMetaProperty = _metaType.PrimaryKey.Properties[0];
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of ImplementationConvention

            protected override void OnInspectDeclaration(ObjectFactoryContext context)
            {
                _proxyTypeKey = (ProxyTypeKey)context.TypeKey;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnImplementPrimaryInterface(ImplementationClassWriter<TypeTemplate.TInterface> writer)
            {
                writer.ImplementInterface<IMongoLazyLoadProxy>();

                using ( TT.CreateScope<TT.TImpl, TT.TKey>(_proxyTypeKey.ImplementationType, _idMetaProperty.ClrType) )
                {
                    writer.Field("$id", out _idField);
                    writer.Field("$object", out _objectField);

                    writer.Constructor<TT.TKey>((cw, id) => {
                        cw.Base();
                        _idField.Assign(id);
                    });

                    writer.Property(_idMetaProperty.ContractPropertyInfo).Implement(
                        p => p.Get(gw => gw.Return(_idField.CastTo<TT.TProperty>())),
                        p => p.Set((sw, value) => _idField.Assign(value.CastTo<TT.TKey>())
                    ));

                    writer.AllProperties().ImplementPropagate(_objectField);
                    writer.AllMethods().ImplementPropagate(_objectField);
                }
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            public ITypeMetadata MetaType
            {
                get { return _metaType; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IPropertyMetadata IdMetaProperty
            {
                get { return _idMetaProperty; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ProxyTypeKey ProxyTypeKey
            {
                get { return _proxyTypeKey; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Field<TypeTemplate.TKey> IdField
            {
                get { return _idField; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Field<TypeTemplate.TInterface> ObjectField
            {
                get { return _objectField; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ProxyEntityObjectConvention : ImplementationConvention
        {
            private readonly ProxyConvention _proxyConvention;
            private Field<IDomainObject> _domainObjectField;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ProxyEntityObjectConvention(ProxyConvention proxyConvention)
                : base(Will.ImplementBaseClass)
            {
                _proxyConvention = proxyConvention;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Field<IDomainObject> DomainObjectField
            {
                get { return _domainObjectField; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of ImplementationConvention

            protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
            {
                var metaType = _proxyConvention.MetaType;
                var keyPropertyInfo = _proxyConvention.IdMetaProperty.ContractPropertyInfo;

                using ( TT.CreateScope<TT.TContract, TT.TKey>(metaType.ContractType, keyPropertyInfo.PropertyType) )
                {
                    var keyBackingField = _proxyConvention.IdField;
                    _domainObjectField = writer.Field<IDomainObject>("$domainObject");

                    writer.ImplementInterfaceExplicitly<IEntityObject>()
                        .Method<IEntityId>(intf => intf.GetId).Implement(f => {
                            f.Return(f.New<EntityId<TT.TContract, TT.TKey>>(keyBackingField));
                        })
                        .Method<object>(intf => intf.SetId).Implement((m, value) => {
                            keyBackingField.Assign(value.CastTo<TT.TKey>());
                        })
                        .Method<IDomainObject>(x => x.SetContainerObject).Implement((m, domainObject) => {
                            _domainObjectField.Assign(domainObject);
                        })
                        .Method<IDomainObject>(x => x.GetContainerObject).Implement(m => {
                            m.Return(_domainObjectField);
                        })
                        .Method(x => x.EnsureDomainObject).Implement(m => {
                        })
                        .Property<EntityState>(x => x.State).Implement(
                            p => p.Get(gw => gw.Return(gw.Const(EntityState.RetrievedPristine)))
                        )
                        .Property<Type>(x => x.ContractType).Implement(
                            p => p.Get(gw => gw.Return(gw.Const(_proxyConvention.MetaType.ContractType)))
                        )
                        .Property<Type>(x => x.FactoryType).Implement(
                            p => p.Get(gw => gw.Return(gw.Const(typeof(MongoLazyLoadProxyFactory))))
                        )
                        .Property<bool>(x => x.IsModified).Implement(
                            p => p.Get(gw => gw.Return(gw.Const(false)))
                        );
                }
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class LazyLoadConvention : DecorationConvention
        {
            private readonly ProxyConvention _proxyConvention;
            private readonly ProxyEntityObjectConvention _entityConvention;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public LazyLoadConvention(ProxyConvention proxyConvention, ProxyEntityObjectConvention entityConvention)
                : base(Will.DecorateMethods | Will.DecorateProperties)
            {
                _proxyConvention = proxyConvention;
                _entityConvention = entityConvention;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of DecorationConvention

            protected override void OnProperty(PropertyMember member, Func<PropertyDecorationBuilder> decorate)
            {
                if ( member.PropertyDeclaration != _proxyConvention.IdMetaProperty.ContractPropertyInfo )
                {
                    decorate().Getter().OnBefore(w => {
                        WriteLazyLoad(w);
                    });
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnMethod(MethodMember member, Func<MethodDecorationBuilder> decorate)
            {
                decorate().OnBefore(w => {
                    WriteLazyLoad(w);
                });
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void WriteLazyLoad(MethodWriterBase writer)
            {
                var w = writer;
                var objectField = _proxyConvention.ObjectField;

                using ( TT.CreateScope<TT.TInterface, TT.TImpl, TT.TKey>(
                    _proxyConvention.ProxyTypeKey.PrimaryInterface, 
                    _proxyConvention.ProxyTypeKey.ImplementationType, 
                    _proxyConvention.IdMetaProperty.ClrType) )
                {
                    w.If(objectField.IsNull()).Then(() => {
                        objectField.Assign(
                            Static.Prop(() => MongoDataRepositoryBase.Current)
                            .Func<string, TT.TKey, TT.TInterface>(x => x.LazyLoadOneByForeignKey<TT.TInterface, TT.TImpl, TT.TKey>,
                                w.Const(_proxyConvention.IdMetaProperty.Name),
                                _proxyConvention.IdField));
                    });
                }
            }
        }
    }
}
