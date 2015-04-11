using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Entities
{
    public interface IDataRepositoryRegistration
    {
        Type DataRepositoryType { get; }
        bool InitializeStorageOnStartup { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class DataRepositoryRegistration<TRepo> : IDataRepositoryRegistration
        where TRepo : class, IApplicationDataRepository
    {
        private bool _initializeStorageOnStartup;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        bool IDataRepositoryRegistration.InitializeStorageOnStartup
        {
            get
            {
                return _initializeStorageOnStartup;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void WithInitializeStorageOnStartup()
        {
            _initializeStorageOnStartup = true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type DataRepositoryType
        {
            get { return typeof(TRepo); }
        }
    }
}
