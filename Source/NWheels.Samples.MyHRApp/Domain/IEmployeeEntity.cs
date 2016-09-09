using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
using NWheels.Entities;
using NWheels.Stacks.MongoDb;

namespace NWheels.Samples.MyHRApp.Domain
{
    [EntityContract]
    public interface IEmployeeEntity
    {
        [PropertyContract.EntityId, PropertyContract.AutoGenerate(typeof(AutoIncrementIntegerIdGenerator))]
        int Id { get; }

        [PropertyContract.Required, PropertyContract.Relation.AggregationParent]
        IDepartmentEntity Department { get; set; }

        [PropertyContract.Required, PropertyContract.Semantic.DisplayName]
        IPersonName Name { get; }

        IPostalAddress Address { get; }
        
        [PropertyContract.Required, PropertyContract.Semantic.EmailAddress]
        string Email { get; set; }

        [PropertyContract.Required, PropertyContract.Semantic.PhoneNumber]
        string Phone { get; set; }
    }
}
