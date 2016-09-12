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
        string Ssn { get; set; }

        [PropertyContract.Required]
        IPersonName Name { get; }

        IPostalAddress Address { get; }
        
        [PropertyContract.Required, PropertyContract.Semantic.EmailAddress]
        string Email { get; set; }

        [PropertyContract.Required, PropertyContract.Semantic.PhoneNumber]
        string Phone { get; set; }

        [PropertyContract.Required, PropertyContract.Relation.Composition]
        IEmploymentEntityPart Employment { get; }

        [PropertyContract.Calculated, PropertyContract.Semantic.DisplayName]
        string FullName { get; }

        [PropertyContract.Calculated]
        string DepartmentName { get; }

        [PropertyContract.Calculated]
        string PositionName { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class EmployeeEntity : IEmployeeEntity
    {
        #region Implementation of IEmployeeEntity

        public abstract string Ssn { get; set; }
        public abstract IPersonName Name { get; }
        public abstract string Email { get; set; }
        public abstract string Phone { get; set; }
        public abstract IPostalAddress Address { get; }
        public abstract IEmploymentEntityPart Employment { get; }

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
                return (Employment.Department != null ? Employment.Department.Name : null);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [EntityImplementation.CalculatedProperty]
        public string PositionName 
        {
            get
            {
                return (Employment.Position != null ? Employment.Position.Name : null);
            }
        }

        #endregion
    }
}
