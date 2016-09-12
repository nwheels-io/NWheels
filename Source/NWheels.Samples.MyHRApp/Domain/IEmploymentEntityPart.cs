using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
using NWheels.Entities;
using NWheels.TypeModel;

namespace NWheels.Samples.MyHRApp.Domain
{
    [EntityPartContract]
    public interface IEmploymentEntityPart
    {
        [PropertyContract.Required, PropertyContract.Relation.AggregationParent]
        IDepartmentEntity Department { get; set; }

        [PropertyContract.Required, PropertyContract.Relation.AggregationParent]
        IPositionEntity Position { get; set; }

        [PropertyContract.Semantic.Date(TimeUnits.YearMonthDay)]
        DateTime ValidFrom { get; set; }

        [PropertyContract.Semantic.Date(TimeUnits.YearMonthDay)]
        DateTime ValidUntil { get; set; }
    }
}
