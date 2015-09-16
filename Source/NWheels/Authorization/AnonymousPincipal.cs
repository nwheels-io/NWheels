using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Authorization
{
    public class AnonymousPrincipal : IPrincipal, IIdentity, IIdentityInfo
    {
        #region Implementation of IPrincipal

        public bool IsOfType(Type accountEntityType)
        {
            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        bool IIdentityInfo.IsInRole(string userRole)
        {
            return (userRole == AnonymousRole);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string[] GetUserRoles()
        {
            return new[] { AnonymousRole };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string UserId
        {
            get { return null; }
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
            return (role == AnonymousRole);
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
            get { return AnonymousRole; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string AuthenticationType
        {
            get { return null; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public bool IsAuthenticated
        {
            get { return false; }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static readonly string AnonymousRole = "anonymous";
        public static readonly AnonymousPrincipal Instance = new AnonymousPrincipal();
    }
}
