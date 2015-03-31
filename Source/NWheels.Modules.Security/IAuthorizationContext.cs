using System;
using System.Collections.Generic;
using System.Linq;
using NWheels.DataObjects;
using NWheels.Entities;

namespace NWheels.Modules.Security
{
    public interface IAuthorizationContext
    {
        bool AuthorizeOperation(IEnumerable<object> authorizedRoles);
        IQueryable<TEntityContract> AuthorizeQuery<TEntityContract>(IQueryable<TEntityContract> repository);
        bool AuthorizeInsert<TEntityContract>(TEntityContract entity);
        bool AuthorizeUpdate<TEntityContract>(TEntityContract entity);
        bool AuthorizeDelete<TEntityContract>(TEntityContract entity);
    }
}
