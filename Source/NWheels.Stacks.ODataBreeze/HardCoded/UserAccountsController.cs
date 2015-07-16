#if false

#pragma warning disable 0618

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using Breeze.ContextProvider;
using Breeze.WebApi2;
using Newtonsoft.Json.Linq;
using NWheels.DataObjects;
using NWheels.Domains.Security;

namespace NWheels.Stacks.ODataBreeze.HardCoded
{
    [BreezeController]
    [EnableCors("*", "*", "*")]
    public class UserAccountsController : ApiController
    {
        private readonly UserAccountsContextProvider _contextProvider;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UserAccountsController(IFramework framework, ITypeMetadataCache metadataCache)
        {
            _contextProvider = new UserAccountsContextProvider(framework, metadataCache);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        public string Metadata()
        {
            var metadataJsonString = _contextProvider.Metadata();
            return metadataJsonString;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        [Queryable]
        public IQueryable<IUserAccountEntity> UserAccount()
        {
            return _contextProvider.QuerySource.AllUsers;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        [Queryable]
        public IQueryable<IUserRoleEntity> UserRole()
        {
            return _contextProvider.QuerySource.UserRoles;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        [Queryable]
        public IQueryable<IOperationPermissionEntity> OperationPermission()
        {
            return _contextProvider.QuerySource.OperationPermissions;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        [Queryable]
        public IQueryable<IEntityAccessRuleEntity> EntityAccessRule()
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

#endif