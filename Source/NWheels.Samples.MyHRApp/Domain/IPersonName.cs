using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
using NWheels.Entities;

namespace NWheels.Samples.MyHRApp.Domain
{
    [EntityPartContract]
    public interface IPersonName
    {
        string Title { get; set; }
     
        [PropertyContract.Required]
        string FirstName { get; set; }

        string MiddleName { get; set; }

        [PropertyContract.Required]
        string LastName { get; set; }

        [PropertyContract.Calculated, PropertyContract.Semantic.DisplayName]
        string FullName { get;  }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class PersonName : IPersonName
    {
        #region Implementation of IPersonName

        public abstract string Title { get; set; }
        public abstract string FirstName { get; set; }
        public abstract string MiddleName { get; set; }
        public abstract string LastName { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string FullName
        {
            get
            {
                return ((FirstName ?? string.Empty) + " " + (LastName ?? string.Empty)).Trim();
            }
        }

        #endregion
    }
}
