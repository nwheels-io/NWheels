using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Breeze.WebApi2;
using NWheels.Domains.Security;

namespace NWheels.Stacks.ODataBreeze.HardCoded
{
    [BreezeController]
    public class UserAccountsController : ApiController
    {
        private readonly IFramework _framework;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UserAccountsController(IFramework framework)
        {
            _framework = framework;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        public IQueryable<IUserAccountEntity> UserAccounts()
        {
            var data = _framework.NewUnitOfWork<IUserAccountDataRepository>();
            return data.AllUsers;
        }
    }
}
