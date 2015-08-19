using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Authorization
{
    public class SystemPrincipal : IPrincipal, IIdentity, IIdentityInfo
    {
        #region Implementation of IPrincipal

        public bool IsOfType(Type accountEntityType)
        {
            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        bool IIdentityInfo.IsInRole(string userRole)
        {
            return (userRole == SystemRole);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string[] GetUserRoles()
        {
            return new[] { SystemRole };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string LoginName
        {
            get { return null; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string QualifiedLoginName
        {
            get { return null; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string PersonFullName
        {
            get { return null; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string EmailAddress
        {
            get { return null; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        bool IPrincipal.IsInRole(string role)
        {
            return (role == SystemRole);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IIdentity Identity
        {
            get { return this; }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IIdentity

        public string Name
        {
            get { return SystemRole; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string AuthenticationType
        {
            get { return SystemRole; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public bool IsAuthenticated
        {
            get { return true; }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static readonly string SystemRole = "SYSTEM";
        public static readonly SystemPrincipal Instance = new SystemPrincipal();
    }
}
