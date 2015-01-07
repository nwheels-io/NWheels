using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Modules.Auth;
using NWheels.Modules.Geo;

namespace NWheels.Modules.Crm
{
    public interface ICustomerEntity
    {
        IOrganizationEntity Organization { get; set; }
        IOrganizationUnitEntity OrganizationUnit { get; set; }
        IUserAccountEntity UserAccount { get; set; }
        IList<IAbstractContactDetailEntity> ContactDetails { get; set; }
        IQueryable<ICustomerRelationshipEntity> Relationships { get; set; }
        IQueryable<IAbstractCustomerActivityEntity> Activities { get; }
    }
}
