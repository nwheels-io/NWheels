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
    public interface IPostalAddress
    {
        [PropertyContract.Required]
        string StreetAddress { get; set; }
        
        [PropertyContract.Required]
        string City { get; set; }
        
        [PropertyContract.Required, PropertyContract.Semantic.PostalCode]
        string ZipCode { get; set; }

        [PropertyContract.Required]
        string Country { get; set; }
    }
}
