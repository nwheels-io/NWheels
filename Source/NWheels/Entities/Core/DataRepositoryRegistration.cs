using System;
using Autofac;
using NWheels.Extensions;

namespace NWheels.Entities.Core
{
    public abstract class DataRepositoryRegistration
    {
        public abstract Type DataRepositoryType { get; }
        public bool HasMultipleDatabases { get; protected set; }
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

        public DataRepositoryRegistration<TRepo> WithMultipleDatabases()
        {
            HasMultipleDatabases = true;
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DataRepositoryRegistration<TRepo> WithInitializeStorageOnStartup()
        {
            _builder.RegisterInstance(new DatabaseInitializationCheckRegistration(typeof(TRepo))).AsSelf();
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override Type DataRepositoryType
        {
            get
            {
                return typeof(TRepo);
            }
        }
    }
}
