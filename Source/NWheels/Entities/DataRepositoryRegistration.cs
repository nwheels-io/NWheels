using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Entities.Impl;
using NWheels.Extensions;

namespace NWheels.Entities
{
    public abstract class DataRepositoryRegistration
    {
        public abstract Type DataRepositoryType { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class DataRepositoryRegistration<TRepo> : DataRepositoryRegistration, AutofacExtensions.IHaveContainerBuilder
        where TRepo : class, IApplicationDataRepository
    {
        private readonly ContainerBuilder _builder;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DataRepositoryRegistration(ContainerBuilder builder)
        {
            _builder = builder;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        ContainerBuilder AutofacExtensions.IHaveContainerBuilder.Builder
        {
            get { return _builder; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DataRepositoryRegistration<TRepo> WithInitializeStorageOnStartup()
        {
            _builder.NWheelsFeatures().Logging().RegisterLogger<DatabaseInitializer.ILogger>();
            _builder.NWheelsFeatures().Hosting().RegisterLifecycleComponent<DatabaseInitializer>();
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override Type DataRepositoryType
        {
            get { return typeof(TRepo); }
        }
    }
}
