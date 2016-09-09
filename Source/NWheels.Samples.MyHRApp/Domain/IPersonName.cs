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
    }
}
