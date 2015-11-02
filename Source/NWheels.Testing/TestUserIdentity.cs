using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Authorization;
using NWheels.Authorization.Core;

namespace NWheels.Testing
{
    public class TestUserIdentity : IIdentityInfo
    {
        private readonly Type _userAccountType;
        private readonly string[] _userRoles;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TestUserIdentity(string userId, string loginName, Type userAccountType, params string[] userRoles)
        {
            _userRoles = userRoles;
            _userAccountType = userAccountType;

            this.UserId = userId;
            this.LoginName = loginName;
            this.Name = loginName;
            this.QualifiedLoginName = loginName;
            this.PersonFullName = loginName;
            this.EmailAddress = loginName + "@email.com";
            this.AuthenticationType = "TEST";
            this.IsAuthenticated = true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IIdentityInfo

        public bool IsOfType(Type accountEntityType)
        {
            return accountEntityType.IsAssignableFrom(_userAccountType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public bool IsInRole(string userRole)
        {
            return _userRoles.Contains(userRole, StringComparer.InvariantCultureIgnoreCase);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public string[] GetUserRoles()
        {
            return _userRoles;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IAccessControlList GetAccessControlList()
        {
            throw new NotSupportedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string UserId { get; private set; }
        public string LoginName { get; private set; }
        public string QualifiedLoginName { get; private set; }
        public string PersonFullName { get; private set; }
        public string EmailAddress { get; private set; }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        #region Implementation of IIdentity

        public string Name { get; set; }
        public string AuthenticationType { get; set; }
        public bool IsAuthenticated { get; set; }

        #endregion
    }
}
