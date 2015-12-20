#pragma warning disable 0618

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Routing;
using Hapil;
using Hapil.Members;
using Hapil.Writers;
using NWheels.Endpoints;
using NWheels.Exceptions;
using NWheels.Extensions;
using System.Web.Http;
using Autofac;
using Breeze.WebApi2;
using NWheels.Conventions.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.Entities;
using NWheels.Entities.Factories;
using TT = Hapil.TypeTemplate;
using CTT = NWheels.Stacks.ODataBreeze.BreezeApiControllerFactory.CustomTypeTemplates;

namespace NWheels.Stacks.ODataBreeze
{
    public class BreezeApiControllerFactory : ConventionObjectFactory
    {
        private readonly ITypeMetadataCache _metadataCache;
        private readonly IDomainObjectFactory _domainObjectFactory;
        private readonly IEntityObjectFactory _persistableObjectFactory;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public BreezeApiControllerFactory(
            DynamicModule module, 
            ITypeMetadataCache metadataCache, 
            IDomainObjectFactory domainObjectFactory, 
            IEntityObjectFactory persistableObjectFactory)
            : base(module)
        {
            _metadataCache = metadataCache;
            _domainObjectFactory = domainObjectFactory;
            _persistableObjectFactory = persistableObjectFactory;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type CreateControllerType(Type dataRepositoryContract)
        {
            var typeKey = new TypeKey(primaryInterface: dataRepositoryContract);
            return base.GetOrBuildType(typeKey).DynamicType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IObjectFactoryConvention[] BuildConventionPipeline(ObjectFactoryContext context)
        {
            return new IObjectFactoryConvention[] {
                new ControllerConvention(_metadataCache, context.TypeKey.PrimaryInterface, _domainObjectFactory, _persistableObjectFactory)
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ControllerConvention : ImplementationConvention
        {
            private readonly ITypeMetadataCache _metadataCache;
            private readonly IDomainObjectFactory _domainObjectFactory;
            private readonly IEntityObjectFactory _persistableObjectFactory;
            private readonly Type _dataRepositoryContract;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ControllerConvention(
                ITypeMetadataCache metadataCache, 
                Type dataRepositoryContract, 
                IDomainObjectFactory domainObjectFactory,
                IEntityObjectFactory persistableObjectFactory)
                : base(Will.InspectDeclaration | Will.ImplementBaseClass)
            {
                _metadataCache = metadataCache;
                _domainObjectFactory = domainObjectFactory;
                _persistableObjectFactory = persistableObjectFactory;
                _dataRepositoryContract = dataRepositoryContract;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnInspectDeclaration(ObjectFactoryContext context)
            {
                context.BaseType = typeof(BreezeApiControllerBase<>).MakeGenericType(_dataRepositoryContract);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
            {
                ImplementConstructor(writer);
                ImplementEntityQueryableActionMethods(writer);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static void ImplementConstructor(ImplementationClassWriter<TypeTemplate.TBase> writer)
            {
                writer.Constructor<IComponentContext, IFramework, ITypeMetadataCache>((cw, components, framework, metadataCache) => 
                    cw.Base(components, framework, metadataCache)
                );
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementEntityQueryableActionMethods(ImplementationClassWriter<TypeTemplate.TBase> writer)
            {
                var repoPropertyByEntityContract = FindEntityRepositoryPropertiesInContract();

                using ( TT.CreateScope<CTT.TDataRepo>(_dataRepositoryContract) )
                {
                    foreach ( var contractPropertyPair in repoPropertyByEntityContract )
                    {
                        ImplementEntityQueryableActionMethod(writer, contractPropertyPair);
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private Dictionary<Type, PropertyInfo> FindEntityRepositoryPropertiesInContract()
            {
                var repoPropertyByEntityContract = new Dictionary<Type, PropertyInfo>();

                TypeMemberCache.Of(_dataRepositoryContract)
                    .ImplementableProperties
                    .Where(IsEntityRepositoryProperty)
                    .ForEach(property => {
                        Type entityContractType;
                        DataRepositoryFactoryBase.ValidateContractProperty(property, out entityContractType);
                        repoPropertyByEntityContract.Add(entityContractType, property);
                    });

                return repoPropertyByEntityContract;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool IsEntityRepositoryProperty(PropertyInfo property)
            {
                return property.PropertyType.IsConstructedGenericTypeOf(typeof(IEntityRepository<>));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementEntityQueryableActionMethod(
                ImplementationClassWriter<TypeTemplate.TBase> writer, 
                KeyValuePair<Type, PropertyInfo> contractPropertyPair)
            {
                var entityContractType = contractPropertyPair.Key;
                var entityRepoProperty = contractPropertyPair.Value;
                var entityMetadata = _metadataCache.GetTypeMetadata(entityContractType);

                var domainObjectImplementationType = _domainObjectFactory.GetOrBuildDomainObjectType(entityContractType);
                var persistableObjectImplementationType = entityMetadata.GetImplementationBy(_persistableObjectFactory.GetType());

                using ( TT.CreateScope<TT.TContract, TT.TImpl, TT.TImpl2>(entityContractType, domainObjectImplementationType, persistableObjectImplementationType) )
                {
                    writer.NewVirtualFunction<IQueryable<TT.TImpl>>(entityRepoProperty.Name).Implement(
                        m => Attributes
                            .Set<HttpGetAttribute>()
                            .Set<EnableBreezeQueryAttribute>()
                            .Set<RouteAttribute>(values => values.Arg<string>(entityMetadata.Name)),
                        w => {
                            var sourceLocal = w.Local<IQueryable<TT.TContract>>();
                            sourceLocal.Assign(
                                w.This<BreezeApiControllerBase<CTT.TDataRepo>>()
                                .Prop<BreezeContextProvider<CTT.TDataRepo>>(x => x.ContextProvider)
                                .Prop<CTT.TDataRepo>(x => x.QuerySource)
                                .Prop<IQueryable<TT.TContract>>(entityRepoProperty));
                            var resultLocal = w.Local<IQueryable<TT.TImpl>>();
                            resultLocal.Assign(w.New<BreezeEndpointQueryable<TT.TContract, TT.TImpl, TT.TImpl2>>(
                                sourceLocal, 
                                w.This<BreezeApiControllerBase<CTT.TDataRepo>>().Prop(x => x.MetadataCache)));
                            w.This<BreezeApiControllerBase<CTT.TDataRepo>>().Void(x => x.CleanupCurrentThread);
                            w.Return(resultLocal);
                        });
                }
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static class CustomTypeTemplates
        {
            // ReSharper disable once InconsistentNaming
            public interface TDataRepo : TypeTemplate.ITemplateType<TDataRepo>, IApplicationDataRepository
            {
            }
        }
    }
}
