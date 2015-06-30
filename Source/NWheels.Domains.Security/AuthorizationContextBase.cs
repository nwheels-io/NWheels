using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Modules.Security
{
    public abstract class AuthorizationContextBase<TUserRole> : IAuthorizationContext
    {
        bool IAuthorizationContext.AuthorizeOperation(IEnumerable<object> authorizedRoles)
        {
            throw new NotImplementedException();
        }

        IQueryable<TEntityContract> IAuthorizationContext.AuthorizeQuery<TEntityContract>(IQueryable<TEntityContract> repository)
        {
            throw new NotImplementedException();
        }

        bool IAuthorizationContext.AuthorizeInsert<TEntityContract>(TEntityContract entity)
        {
            throw new NotImplementedException();
        }

        bool IAuthorizationContext.AuthorizeUpdate<TEntityContract>(TEntityContract entity)
        {
            throw new NotImplementedException();
        }

        bool IAuthorizationContext.AuthorizeDelete<TEntityContract>(TEntityContract entity)
        {
            throw new NotImplementedException();
        }



        protected virtual bool OnAuthorizeOperation(IEnumerable<TUserRole> authorizedRoles)
        {
            return true;
        }

        protected virtual void BuildEntityAccessRules(IEntityAccessControlBuilder access)
        {
        }

        #if false
        private readonly Dictionary<Type, IEntityAccessRule> _accessControlByContractType;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected AuthorizationContextBase()
        {
            _accessControlByContractType = new Dictionary<Type, IEntityAccessRule>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual bool AuthorizeOperation(IEnumerable<TUserRole> allowedRoles)
        {
            return true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IQueryable<TEntityContract> AuthorizeQuery<TEntityContract>(IQueryable<TEntityContract> repository)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual bool AuthorizeInsert<TEntityContract>(TEntityContract entity)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual bool AuthorizeDelete<TEntityContract>(TEntityContract entity)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual bool AuthorizeUpdate<TEntityContract>(TEntityContract entity)
        {
        }
#endif

        //-----------------------------------------------------------------------------------------------------------------------------------------------------


        //-----------------------------------------------------------------------------------------------------------------------------------------------------

    }
}
