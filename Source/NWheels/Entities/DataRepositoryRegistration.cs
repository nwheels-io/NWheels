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
        public abstract bool ShouldInitializeStorageOnStartup { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class DataRepositoryRegistration<TRepo> : DataRepositoryRegistration, AutofacExtensions.IHaveContainerBuilder
        where TRepo : class, IApplicationDataRepository
    {
        private readonly ContainerBuilder _builder;
        private bool _shouldInitializeStorageOnStartup;

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
            _shouldInitializeStorageOnStartup = true;
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool ShouldInitializeStorageOnStartup 
        {
            get
            {
                return _shouldInitializeStorageOnStartup;
            }
        }
    }
}
