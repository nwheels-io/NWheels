using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Autofac;
using Breeze.ContextProvider;
using Hapil;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Library;
using NWheels.Concurrency;
using NWheels.Conventions.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.Extensions;
using NWheels.Entities;
using NWheels.Entities.Core;

namespace NWheels.Stacks.ODataBreeze
{
    public class BreezeContextProvider<TDataRepo> : ContextProvider, IDisposable
        where TDataRepo : class, IApplicationDataRepository
    {
        private readonly IComponentContext _components;
        private readonly IFramework _framework;
        private readonly ITypeMetadataCache _metadataCache;
        private readonly IBreezeEndpointLogger _logger;
        private readonly TDataRepo _querySource;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public BreezeContextProvider(IComponentContext components, IFramework framework, ITypeMetadataCache metadataCache, IBreezeEndpointLogger logger)
        {
            _components = components;
            _framework = framework;
            _metadataCache = metadataCache;
            _logger = logger;

            //TODO: remove this once we are sure the bug is solved
            PerContextResourceConsumerScope<TDataRepo> stale;
            if ( (stale = new ThreadStaticAnchor<PerContextResourceConsumerScope<TDataRepo>>().Current) != null )
            {
                _logger.StaleUnitOfWorkEncountered(stale.Resource.ToString(), ((DataRepositoryBase)(object)stale.Resource).InitializerThreadText);
            }

            _logger.CreatingQuerySource(domainContext: typeof(TDataRepo).Name);
            _querySource = framework.NewUnitOfWork<TDataRepo>(autoCommit: false);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IDisposable

        public void Dispose()
        {
            _logger.DisposingQuerySource(domainContext: typeof(TDataRepo).Name);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TDataRepo QuerySource
        {
            get { return _querySource; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string BuildJsonMetadata()
        {
            var builder = new BreezeMetadataBuilder(_metadataCache, _components.Resolve<IDomainObjectFactory>(), _components.Resolve<IEntityObjectFactory>());

            builder.AddDataService("rest/");

            foreach ( var contractType in _querySource.GetEntityContractsInRepository() )
            {
                builder.AddEntity(contractType);
            }

            return builder.GetMetadataJsonString();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void CloseDbConnection()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override System.Data.IDbConnection GetDbConnection()
        {
            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OpenDbConnection()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void SaveChangesCore(SaveWorkState saveWorkState)
        {
            saveWorkState.KeyMappings = new List<KeyMapping>();

            using ( var data = _framework.NewUnitOfWork<TDataRepo>() )
            {
                var entityRepositories = data.GetEntityRepositories().Where(repo => repo != null).ToDictionary(repo => repo.ContractType);

                foreach ( var typeGroup in saveWorkState.SaveMap )
                {
                    foreach ( var entityToSave in typeGroup.Value )
                    {
                        var entityRepository = entityRepositories[entityToSave.Entity.As<IObject>().ContractType];

                        switch ( entityToSave.EntityState )
                        {
                            case Breeze.ContextProvider.EntityState.Added:
                                MapAutoGeneratedKey(saveWorkState, entityToSave, entityRepository);
                                entityRepository.Insert(entityToSave.Entity);
                                break;
                            case Breeze.ContextProvider.EntityState.Modified:
                                entityRepository.Update(entityToSave.Entity);
                                break;
                            case Breeze.ContextProvider.EntityState.Deleted:
                                entityRepository.Delete(entityToSave.Entity);
                                break;
                        }
                    }
                }

                data.CommitChanges();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void MapAutoGeneratedKey(SaveWorkState saveWorkState, EntityInfo entityToSave, IEntityRepository entityRepository)
        {
            if ( entityToSave.AutoGeneratedKey != null && entityToSave.AutoGeneratedKey.AutoGeneratedKeyType != AutoGeneratedKeyType.None )
            {
                entityToSave.AutoGeneratedKey.TempValue = EntityId.ValueOf(entityToSave.Entity);
                
                var tempEntity = entityRepository.New(); // will be initialized with auto-generated id
                var newEntityId = EntityId.ValueOf(tempEntity);
                
                ((IEntityObject)entityToSave.Entity).SetId(newEntityId);
                entityToSave.AutoGeneratedKey.RealValue = newEntityId;
                
                saveWorkState.KeyMappings.Add(new KeyMapping() {
                    EntityTypeName = BreezeMetadataBuilder.GetQualifiedTypeString(entityToSave.Entity.GetType()),
                    TempValue = entityToSave.AutoGeneratedKey.TempValue,
                    RealValue = entityToSave.AutoGeneratedKey.RealValue
                });
            }
        }
    }
}
