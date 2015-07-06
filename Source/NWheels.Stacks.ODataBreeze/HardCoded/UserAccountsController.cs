using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Breeze.ContextProvider;
using Breeze.WebApi2;
using Newtonsoft.Json.Linq;
using NWheels.DataObjects;
using NWheels.Domains.Security;

namespace NWheels.Stacks.ODataBreeze.HardCoded
{
    [BreezeController]
    public class UserAccountsController : ApiController
    {
        private readonly UserAccountsContextProvider _contextProvider;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UserAccountsController(IFramework framework, ITypeMetadataCache metadataCache)
        {
            _contextProvider = new UserAccountsContextProvider(framework, metadataCache);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        // ~/breeze/todos/Metadata 
        [HttpGet]
        public string Metadata()
        {
            var metadata = _contextProvider.Metadata();
            return metadata;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        public IQueryable<IUserAccountEntity> UserAccounts()
        {
            return _contextProvider.QuerySource.AllUsers;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        public IQueryable<IUserRoleEntity> UserRoles()
        {
            return _contextProvider.QuerySource.UserRoles;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        public IQueryable<IOperationPermissionEntity> OperationPermissions()
        {
            return _contextProvider.QuerySource.OperationPermissions;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        public IQueryable<IEntityAccessRuleEntity> EntityAccessRules()
        {
            return _contextProvider.QuerySource.EntityAccessRules;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        [HttpPost]
        public SaveResult SaveChanges(JObject saveBundle)
        {
            return _contextProvider.SaveChanges(saveBundle);
        }
    }
}
