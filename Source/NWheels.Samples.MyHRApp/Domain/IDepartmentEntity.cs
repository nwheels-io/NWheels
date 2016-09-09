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
    public interface IDepartmentEntity
    {
        [PropertyContract.AutoGenerate(typeof(AutoIncrementIntegerIdGenerator))]
        int Id { get; }

        [PropertyContract.Required, PropertyContract.Semantic.DisplayName]
        string Name { get; set; }

        [PropertyContract.Relation.Aggregation, PropertyContract.Relation.ManyToOne]
        IEmployeeEntity Manager { get; set; }
    }
}
