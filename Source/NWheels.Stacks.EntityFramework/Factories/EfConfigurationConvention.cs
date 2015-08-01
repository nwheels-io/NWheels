using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.DataObjects.Core.Factories;
using TT = Hapil.TypeTemplate;

namespace NWheels.Stacks.EntityFramework.Factories
{
    public class EfConfigurationConvention : ImplementationConvention
    {
        private readonly ITypeMetadataCache _metadataCache;
        private readonly ITypeMetadata _metaType;
        private readonly PropertyImplementationStrategyMap _propertyMap;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EfConfigurationConvention(ITypeMetadataCache metadataCache, ITypeMetadata metaType, PropertyImplementationStrategyMap propertyMap)
            : base(Will.ImplementBaseClass)
        {
            _metadataCache = metadataCache;
            _metaType = metaType;
            _propertyMap = propertyMap;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
        {
            using ( TT.CreateScope<TT.TImpl>(writer.OwnerClass.TypeBuilder) )
            {
                writer.NewStaticVoidMethod<ITypeMetadataCache, DbModelBuilder>("ConfigureEfModel", arg1: "metadataCache", arg2: "modelBuilder")
                    .Implement((w, metadataCache, builder) => {
                        var typeMetadataLocal = w.Local<ITypeMetadata>();
                        var entityTypeConfigLocal = w.Local<EntityTypeConfiguration<TT.TImpl>>();

                        typeMetadataLocal.Assign(metadataCache.Func<Type, ITypeMetadata>(x => x.GetTypeMetadata, w.Const(_metaType.ContractType)));
                        entityTypeConfigLocal.Assign(Static.Func(EfModelApi.EntityType<TT.TImpl>, builder, typeMetadataLocal));

                        foreach ( var strategy in _propertyMap.Strategies.OfType<EfPropertyImplementationStrategy>() )
                        {
                            strategy.Configurator.OnWritingEfModelConfiguration(w, builder, typeMetadataLocal, entityTypeConfigLocal);
                        }
                    });
            }
        }
    }
}
