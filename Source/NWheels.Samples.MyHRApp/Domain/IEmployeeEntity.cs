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
        [PropertyContract.EntityId]
        string Id { get; set; }

        [PropertyContract.Required, PropertyContract.Relation.AggregationParent]
        IDepartmentEntity Department { get; set; }

        [PropertyContract.Required]
        IPersonName Name { get; }

        IPostalAddress Address { get; }
        
        [PropertyContract.Required, PropertyContract.Semantic.EmailAddress]
        string Email { get; set; }

        [PropertyContract.Required, PropertyContract.Semantic.PhoneNumber]
        string Phone { get; set; }

        [PropertyContract.Calculated, PropertyContract.Semantic.DisplayName]
        string FullName { get; }

        [PropertyContract.Calculated]
        string DepartmentName { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class EmployeeEntity : IEmployeeEntity
    {
        #region Implementation of IEmployeeEntity

        public abstract string Id { get; set; }
        public abstract IDepartmentEntity Department { get; set; }
        public abstract IPersonName Name { get; }
        public abstract IPostalAddress Address { get; }
        public abstract string Email { get; set; }
        public abstract string Phone { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [EntityImplementation.CalculatedProperty]
        public string FullName
        {
            get { return Name.FullName; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [EntityImplementation.CalculatedProperty]
        public string DepartmentName
        {
            get
            {
                return (Department != null ? Department.Name : null);
            }
        }

        #endregion
    }
}
