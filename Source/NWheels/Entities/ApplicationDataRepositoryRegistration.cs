using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Entities
{
    public abstract class ApplicationDataRepositoryRegistration
    {
        public abstract Type DataRepositoryType { get; }
        public abstract bool InitializeOnStartup { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class ApplicationDataRepositoryRegistration<TRepo> : ApplicationDataRepositoryRegistration
        where TRepo : class, IApplicationDataRepository
    {
        private readonly bool _initializeStorageOnStartup;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ApplicationDataRepositoryRegistration(bool initializeStorageOnStartup)
        {
            _initializeStorageOnStartup = initializeStorageOnStartup;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override Type DataRepositoryType
        {
            get { return typeof(TRepo); }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool InitializeOnStartup
        {
            get { return _initializeStorageOnStartup; }
        }
    }
}
