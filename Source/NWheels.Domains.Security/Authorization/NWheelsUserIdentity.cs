using System.Security.Principal;

namespace NWheels.Domains.Security.Authorization
{
    public class NWheelsUserIdentity : IIdentity
    {
        private readonly IUserAccountEntity _user;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NWheelsUserIdentity(IUserAccountEntity user)
        {
            _user = user;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string AuthenticationType
        {
            get { return "nwheels"; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsAuthenticated
        {
            get { return true; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Name
        {
            get { return _user.LoginName; }
        }
    }
}
