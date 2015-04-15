using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Extensions;

namespace NWheels.Entities
{
    public abstract class DataRepositoryRegistration
    {
        public abstract Type DataRepositoryType { get; }
        public abstract bool InitializeStorageOnStartup { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class DataRepositoryRegistration<TRepo> : DataRepositoryRegistration, AutofacExtensions.IHaveContainerBuilder
        where TRepo : class, IApplicationDataRepository
    {
        private readonly ContainerBuilder _builder;
        private bool _initializeStorageOnStartup;

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

        public override bool InitializeStorageOnStartup
        {
            get
            {
                return _initializeStorageOnStartup;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DataRepositoryRegistration<TRepo> WithInitializeStorageOnStartup()
        {
            _initializeStorageOnStartup = true;
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override Type DataRepositoryType
        {
            get { return typeof(TRepo); }
        }
    }
}
