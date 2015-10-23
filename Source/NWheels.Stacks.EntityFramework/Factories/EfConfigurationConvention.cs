using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Members;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.DataObjects.Core.Factories;
using NWheels.Extensions;
using TT = Hapil.TypeTemplate;

namespace NWheels.Stacks.EntityFramework.Factories
{
    public class EfConfigurationConvention : ImplementationConvention
    {
        public const string DiscriminatorColumnName = "_t";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly ITypeMetadataCache _metadataCache;
        private readonly ITypeMetadata _metaType;
        private readonly PropertyImplementationStrategyMap _propertyMap;
        private ObjectFactoryContext _facotryContext;
        private Local<ITypeMetadata> _typeMetadataLocal;
        private Local<EntityTypeConfiguration<TypeTemplate.TImpl>> _entityTypeConfigLocal;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EfConfigurationConvention(ITypeMetadataCache metadataCache, ITypeMetadata metaType, PropertyImplementationStrategyMap propertyMap)
            : base(Will.InspectDeclaration | Will.ImplementBaseClass)
        {
            _metadataCache = metadataCache;
            _metaType = metaType;
            _propertyMap = propertyMap;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnInspectDeclaration(ObjectFactoryContext context)
        {
            _facotryContext = context;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
        {
            using ( TT.CreateScope<TT.TImpl>(writer.OwnerClass.TypeBuilder) )
            {
                writer.NewStaticVoidMethod<ITypeMetadataCache, DbModelBuilder>("ConfigureEfModel", arg1: "metadataCache", arg2: "modelBuilder")
                    .Implement((w, metadataCache, builder) => {
                        if ( _metaType.ContractType.IsEntityContract() )
                        {
                            ConfigureEntityType(w, metadataCache, builder);
                        }
                        else
                        {
                            ConfigureComplexType(w, metadataCache, builder);
                        }
                    });
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ConfigureEntityType(VoidMethodWriter w, Argument<ITypeMetadataCache> metadataCache, Argument<DbModelBuilder> builder)
        {
            _typeMetadataLocal = w.Local<ITypeMetadata>();
            _entityTypeConfigLocal = w.Local<EntityTypeConfiguration<TT.TImpl>>();

            _typeMetadataLocal.Assign(metadataCache.Func<Type, ITypeMetadata>(x => x.GetTypeMetadata, w.Const(_metaType.ContractType)));

            if ( IsInheritanceConfigurationRequired() )
            {
                WriteInheritanceConfiguration(w, builder, _typeMetadataLocal);
            }
            else
            {
                _entityTypeConfigLocal.Assign(Static.Func(EfModelApi.EntityType<TT.TImpl>, builder, _typeMetadataLocal, metadataCache));
            }

            WritePropertyConfigurations(w, builder, metadataCache);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool IsInheritanceConfigurationRequired()
        {
            return (_metaType.BaseType != null && !_metaType.IsAbstract);
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void WriteInheritanceConfiguration(VoidMethodWriter w, Argument<DbModelBuilder> builder, Local<ITypeMetadata> typeMetadataLocal)
        {
            ITypeMetadata rootBaseType;
            var rootBaseTypeImplementation = GetRootBaseTypeImplementation(w.OwnerClass, out rootBaseType);
            var discriminatorValue = GetDiscriminatorValue(rootBaseType, _metaType);

            using ( TT.CreateScope<TT.TContract>(rootBaseTypeImplementation) )
            {
                _entityTypeConfigLocal.Assign(Static.Func(EfModelApi.InheritedEntityType<TT.TContract, TT.TImpl>, 
                    builder, 
                    typeMetadataLocal, 
                    w.Const(DiscriminatorColumnName),
                    w.Const(discriminatorValue)));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void WritePropertyConfigurations(VoidMethodWriter w, Argument<DbModelBuilder> builder, Argument<ITypeMetadataCache> metadataCache)
        {
            foreach ( var strategy in _propertyMap.Strategies.OfType<EfPropertyImplementationStrategy>() )
            {
                strategy.Configurator.OnWritingEfModelConfiguration(w, builder, _typeMetadataLocal, metadataCache, _entityTypeConfigLocal);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Type GetRootBaseTypeImplementation(ClassType thisClassType, out ITypeMetadata rootBaseType)
        {
            rootBaseType = _metaType.GetRootBaseType();
            
            Type rootBaseTypeImplementation;

            if ( _metaType != rootBaseType )
            {
                rootBaseTypeImplementation = _facotryContext.Factory.FindDynamicType(rootBaseType.ContractType);
            }
            else
            {
                rootBaseTypeImplementation = thisClassType.TypeBuilder;
            }

            return rootBaseTypeImplementation;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ConfigureComplexType(VoidMethodWriter w, Argument<ITypeMetadataCache> metadataCache, Argument<DbModelBuilder> builder)
        {
            Static.Void(EfModelApi.ComplexType<TT.TImpl>, builder);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string GetDiscriminatorValue(ITypeMetadata rootBaseType, ITypeMetadata derivedType)
        {
            return (derivedType.BaseType != null ? derivedType.Name.TrimTail(rootBaseType.Name) : derivedType.Name);
        }
    }
}
