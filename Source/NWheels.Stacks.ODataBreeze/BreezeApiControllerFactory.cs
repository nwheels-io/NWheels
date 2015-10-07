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
using NWheels.Conventions.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.Entities;
using TT = Hapil.TypeTemplate;
using CTT = NWheels.Stacks.ODataBreeze.BreezeApiControllerFactory.CustomTypeTemplates;

namespace NWheels.Stacks.ODataBreeze
{
    public class BreezeApiControllerFactory : ConventionObjectFactory
    {
        private readonly ITypeMetadataCache _metadataCache;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public BreezeApiControllerFactory(DynamicModule module, ITypeMetadataCache metadataCache)
            : base(module)
        {
            _metadataCache = metadataCache;
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
                new ControllerConvention(_metadataCache, dataRepositoryContract: context.TypeKey.PrimaryInterface)
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ControllerConvention : ImplementationConvention
        {
            private readonly ITypeMetadataCache _metadataCache;
            private readonly Type _dataRepositoryContract;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ControllerConvention(ITypeMetadataCache metadataCache, Type dataRepositoryContract)
                : base(Will.InspectDeclaration | Will.ImplementBaseClass)
            {
                _metadataCache = metadataCache;
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

                using ( TT.CreateScope<TT.TContract>(entityContractType) )
                {
                    writer.NewVirtualFunction<IQueryable<TT.TContract>>(entityRepoProperty.Name).Implement(
                        m => Attributes
                            .Set<HttpGetAttribute>()
                            .Set<QueryableAttribute>()
                            .Set<RouteAttribute>(values => values.Arg<string>(entityMetadata.Name)),
                        w => w.Return(
                            w.This<BreezeApiControllerBase<CTT.TDataRepo>>()
                            .Prop<BreezeContextProvider<CTT.TDataRepo>>(x => x.ContextProvider)
                            .Prop<CTT.TDataRepo>(x => x.QuerySource)
                            .Prop<IQueryable<TT.TContract>>(entityRepoProperty)));
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
